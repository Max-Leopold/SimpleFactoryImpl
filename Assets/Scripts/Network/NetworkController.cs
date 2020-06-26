using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Messages.Players;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects;
using SimpleFactoryServerLib.Objects.Materials;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using SimpleFactoryServerLib.Server;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityStandardAssets.Characters.FirstPerson;

public class NetworkController : MonoBehaviour
{
    public GameSettings settings;

    // Prefabs
    public GameObject emptyAssemblyLine;
    public GameObject playerPrefab;
    public GameObject oreOutputMachine;
    public GameObject splitter;
    public GameObject combiner;
    public GameObject ore;
    public GameObject processingMachine;
    public GameObject chest;
    public GameObject oil;

    private ConcurrentQueue<NetworkMessage> inboundMessages = new ConcurrentQueue<NetworkMessage>();
    private ConcurrentDictionary<int, GameObject> worldObjects = new ConcurrentDictionary<int, GameObject>();
    private ConcurrentDictionary<int, GameObject> players = new ConcurrentDictionary<int, GameObject>();
    private ConcurrentDictionary<long, GameObject> droppedMaterials = new ConcurrentDictionary<long, GameObject>();
    private Server server;
    private List<Thread> threads = new List<Thread>();

    private static int USER_ID = -1;
    private static string USERNAME = Environment.UserName;
    private StaticGameVariables staticGameVariables;
    private byte[] _buffer;
    private bool _disconnected;
    private FirstPersonController fpsController;

    private void OnDestroy()
    {
        if (server != null)
            server.stop();
        staticGameVariables.Socket.Disconnect(false);
        foreach (Thread thread in threads)
        {
            thread.Abort();
            thread.Interrupt();
        }
    }

    private void Awake()
    {
        GameObject staticGameVariablesObject = GameObject.Find("StaticGameVariables");

        staticGameVariables = staticGameVariablesObject.GetComponent<StaticGameVariables>();
    }


    void Start()
    {
        try
        {
            // Send the Connection Message
            ConnectionMessage conMsg =
                new ConnectionMessage(Environment.UserName, staticGameVariables.Ip.Address.MapToIPv4().ToString(),
                    staticGameVariables.Ip.Port);
            sendToServer(conMsg);
            Debug.Log("Connected");
            new Thread(handleTcpServerMessages).Start();
            new Thread(handleUdpServerMessages).Start();
            //new Thread(pingThread).Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Network connect failed");
            Debug.LogError("Can't connect to " + staticGameVariables.ServerIpAdress);
            Debug.LogError(e);
        }
    }

    private void Update()
    {
        if (_disconnected)
        {
            fpsController.m_MouseLook.SetCursorLock(false);
            fpsController.enabled = false;
            loadStartMenuScene();
            _disconnected = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            WorldObject w = worldObjects[1].GetComponent<NetworkObject>().WorldObject;
            w.rotation = new Rotation(0, w.rotation.y + 90, 0);
            sendToServer(new InstantiateMessage(w));
        }

        NetworkMessage networkMessage;

        ConcurrentQueue<NetworkMessage> tmpQueue = new ConcurrentQueue<NetworkMessage>();

        while (!inboundMessages.IsEmpty)
        {
            inboundMessages.TryDequeue(out networkMessage);
            if (networkMessage is GameState)
            {
                tmpQueue.Enqueue(networkMessage);
                GameState gameState = (GameState) networkMessage;
                handleGameState(gameState);
            }
            else if (networkMessage is PlayerDisconnectedMessage)
            {
                PlayerDisconnectedMessage pDm = (PlayerDisconnectedMessage) networkMessage;
                Debug.Log("Received disconnect from player: " + pDm.userName);
                if (players.ContainsKey(pDm.userID))
                {
                    GameObject player;
                    players.TryRemove(pDm.userID, out player);
                    Destroy(player);
                }
            }
            else if (networkMessage is ConnectionMessage)
            {
                ConnectionMessage conMsg = (ConnectionMessage) networkMessage;
                USER_ID = conMsg.userID;
                Debug.Log("Server returned the connection message, USER ID is: " + USER_ID);
            }
            else if (networkMessage is PlayerPositionUpdate)
            {
                PlayerPositionUpdate posUpdate = (PlayerPositionUpdate) networkMessage;
                if (players.ContainsKey(posUpdate.userID))
                {
                    if (posUpdate.userID != USER_ID)
                    {
                        players[posUpdate.userID].GetComponent<NetworkPlayer>().updatePosition(posUpdate);
                    }
                }
                else
                {
                    if (posUpdate.userID == USER_ID)
                    {
                        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                        player.GetComponent<NetworkPlayer>().username = posUpdate.userName;
                        player.GetComponent<NetworkPlayer>().userId = posUpdate.userID;
                        player.GetComponent<NetworkPlayer>().networkController = this;
                        player.GetComponent<NetworkPlayer>().IsRemotePlayer = false;
                        players.TryAdd(posUpdate.userID, player);
                        fpsController = player.GetComponent<FirstPersonController>();
                    }
                    else
                    {
                        Debug.Log("Creating new player");
                        GameObject player = Instantiate(playerPrefab, convertPosToVector3(posUpdate.position),
                            convertRotToQuaternion(posUpdate.rotation));
                        player.GetComponent<NetworkPlayer>().username = posUpdate.userName;
                        player.GetComponent<NetworkPlayer>().userId = posUpdate.userID;
                        player.GetComponent<NetworkPlayer>().networkController = this;
                        player.GetComponent<NetworkPlayer>().IsRemotePlayer = true;
                        players.TryAdd(posUpdate.userID, player);
                    }
                }
            }
            else if (networkMessage is DeleteMessage)
            {
                //Debug.LogError("Received Deletemessage " + networkMessage);
                DeleteMessage dMsg = (DeleteMessage) networkMessage;
                if (worldObjects.ContainsKey(dMsg.worldObject.id))
                {
                    GameObject toRemove;
                    worldObjects.TryRemove(dMsg.worldObject.id, out toRemove);

                    if (toRemove != null)
                    {
                        if (dMsg.worldObject is ProcessingWorldObject)
                        {
                            InputOutputManager iom = toRemove.GetComponent<BuildableObject>().inputOutputManager;
                            if (iom.Input != null)
                            {
                                
                                iom.Input.GetComponent<InputOutputManager>().Output = null;
                                iom.Input.GetComponent<InputOutputManager>().actualOutputs.Remove(iom.gameObject);
                                iom.Input.GetComponent<InputOutputManager>().actualOutputs = iom.Input
                                        .GetComponent<InputOutputManager>()
                                        .actualOutputs.FindAll(e => e != null);
                                
                            }

                            if (iom.Output != null)
                            {
                                
                                iom.Output.GetComponent<InputOutputManager>().Input = null;
                                iom.Output.GetComponent<InputOutputManager>().actualInputs.Remove(iom.gameObject);
                                iom.Output.GetComponent<InputOutputManager>().actualInputs = iom.Output
                                        .GetComponent<InputOutputManager>().actualInputs.FindAll(e => e != null);
                                
                            }
                        }

                        //Debug.LogError("Going to delete " + toRemove + " at: " + toRemove.transform.position);
                        Destroy(toRemove);
                        //Debug.LogError("Deleted object ");
                    }
                }
            }
            else if (networkMessage is DroppedMaterialMessage)
            {
                Debug.Log("Received dropped material message");
                DroppedMaterialMessage dm = (DroppedMaterialMessage) networkMessage;
                if (dm.material == null && droppedMaterials.ContainsKey(dm.id)) // Delete
                {
                    GameObject mat = droppedMaterials[dm.id];
                    Destroy(mat);
                }
                else if (dm.material != null && !droppedMaterials.ContainsKey(dm.id))
                {
                    AbstractMaterial material = settings.convertMaterialToUnityMaterial(dm.material);
                    GameObject droppedMaterial = Instantiate(material.prefab, convertPosToVector3(dm.position),
                        Quaternion.identity);
                    MaterialManager mm = droppedMaterial.GetComponent<MaterialManager>();
                    mm.amount = dm.amount;
                    mm.material = material;
                    mm.id = dm.id;
                    droppedMaterials.TryAdd(dm.id, droppedMaterial);
                }
            }
        }

        while (tmpQueue.Count != 0)
        {
            tmpQueue.TryDequeue(out networkMessage);

            if (networkMessage is GameState)
            {
                GameState gameState = (GameState) networkMessage;
                setOutputs(gameState);
            }
        }
    }

    private void setOutputs(GameState gameState)
    {
        foreach (WorldObject worldObject in gameState.objects)
        {
            if (worldObjects.ContainsKey(worldObject.id))
            {
                if (worldObject is SingleOutputWorldObject)
                {
                    SingleOutputWorldObject singleOutputWorldObject =
                        (SingleOutputWorldObject) worldObject;

                    GameObject g = worldObjects[worldObject.id];

                    if (singleOutputWorldObject.next != null &&
                        worldObjects.ContainsKey(singleOutputWorldObject.next.id))
                    {
                        Debug.Log("Handle Game State >> Set " + g.transform.position + " output to " +
                                  worldObjects[singleOutputWorldObject.next.id].transform.position);
                        g.GetComponent<BuildableObject>().inputOutputManager
                            .setOutput(worldObjects[singleOutputWorldObject.next.id]);
                    }
                    else
                    {
                        Debug.Log("Handle Game State >> Could not find " + singleOutputWorldObject.next +
                                  " in Dictionary");
                    }
                }
                else if (worldObject is MultiOutputWorldObject)
                {
                    MultiOutputWorldObject multiOutputWorldObject =
                        (MultiOutputWorldObject) worldObject;

                    GameObject g = worldObjects[worldObject.id];

                    if (multiOutputWorldObject.next != null && multiOutputWorldObject.next.Count > 0)
                    {
                        foreach (var processingWorldObject in multiOutputWorldObject.next)
                        {
                            if (worldObjects.ContainsKey(processingWorldObject.id))
                            {
                                g.GetComponent<BuildableObject>().inputOutputManager
                                    .setOutput(worldObjects[processingWorldObject.id]);
                            }
                        }
                    }
                }
            }
        }
    }

    private void pingThread()
    {
        while (!_disconnected)
        {
            sendToServer(new PingMessage());
            Thread.Sleep(1500);
        }
    }

    /**
     * Sends a network message to the server using UDP
     * Used for Player Positions
     */
    public void sendToServerUdp(NetworkMessage networkMessage)
    {
        try
        {
            networkMessage.userID = USER_ID;
            networkMessage.userName = USERNAME;
            DataMessage dmg = NetworkConverter.Serialize(networkMessage);
            staticGameVariables.UdpClient.Send(dmg.Data, dmg.Data.Length, SocketFlags.None);
        }
        catch (SocketException ex)
        {
            Debug.LogError(ex);
            setDisconnectedForMainThread();
        }
    }

    /**
     * Sends a network message to the server using TCP
     */
    public void sendToServer(NetworkMessage networkMessage)
    {
        try
        {
            networkMessage.userName = USERNAME;
            networkMessage.userID = USER_ID;
            DataMessage message = NetworkConverter.Serialize(networkMessage);
            byte[] dHeader = BitConverter.GetBytes(message.Data.Length);
            staticGameVariables.Socket.Send(dHeader);
            staticGameVariables.Socket.Send(message.Data);
        }
        catch (SocketException ex)
        {
            Debug.LogError(ex);
            setDisconnectedForMainThread();
        }
    }

    private void handleTcpServerMessages()
    {
        while (staticGameVariables.Socket.Connected)
        {
            if (staticGameVariables.Socket.Connected)
            {
                try
                {
                    _buffer = new byte[Consts.MSG_HEADER_SIZE];
                    int readCount = 0;
                    int remainingSize = Consts.MSG_HEADER_SIZE;
                    do
                    {
                        readCount += staticGameVariables.Socket.Receive(_buffer, readCount, remainingSize, SocketFlags.None);
                        remainingSize = 4 - readCount;
                        // Console.WriteLine("Remaining: " + remainingSize);
                    } while (remainingSize > 0);
                    int headerSize = BitConverter.ToInt32(_buffer, 0);
                    //Debug.Log("Received header: " + headerSize);
                    _buffer = new byte[headerSize];
                    readCount = 0;
                    remainingSize = headerSize;
                    do
                    {
                        readCount +=
                            staticGameVariables.Socket.Receive(_buffer, readCount, remainingSize, SocketFlags.None);
                        remainingSize = headerSize - readCount;
                        //Console.WriteLine("Remaining: " + remainingSize);
                    } while (remainingSize > 0);

                    if (readCount != headerSize)
                    {
                        Debug.LogError("Message not fully read!! (" + readCount + "/" + headerSize + ")");
                        continue;
                    }
                    //socket.Receive(buffer, 0, headerSize, 0);
                    //Debug.Log("Parsing message");
                    NetworkMessage message = NetworkConverter.Deserialize(new DataMessage(_buffer));
                    //Debug.Log("Received: " + message.GetType());
                    inboundMessages.Enqueue(message);
                }
                catch (SocketException ex)
                {
                    Debug.LogError(ex);
                    setDisconnectedForMainThread();
                }
            }
            else
            {
                setDisconnectedForMainThread();
            }
        }
    }

    private void handleUdpServerMessages()
    {
        while (staticGameVariables.Socket.Connected)
        {
            byte[] buffer = new byte[8192];
            staticGameVariables.UdpClient.Receive(buffer, buffer.Length, SocketFlags.None);
            DataMessage dmg = new DataMessage(buffer);
            NetworkMessage networkMessage = NetworkConverter.Deserialize(dmg);
            Debug.Log(" >> Received per UDP: " + networkMessage.GetType());
            inboundMessages.Enqueue(networkMessage);
        }
    }

    private void handleGameState(GameState state)
    {
        foreach (WorldObject wObject in state.objects)
        {
            if (!worldObjects.ContainsKey(wObject.id)) // Instantiate objects
            {
                Debug.Log("Object: " + wObject.id + " not in list...");
                if (wObject is SimpleFactoryServerLib.Objects.AssemblyLine) // Instantiate Assembly Line
                {
                    SimpleFactoryServerLib.Objects.AssemblyLine wObjectAssemblyLine =
                        (SimpleFactoryServerLib.Objects.AssemblyLine) wObject;

                    GameObject assemblyLinePrefab = emptyAssemblyLine;
                    Debug.Log("Cast Assembly Line: " + wObjectAssemblyLine.Prefab);

                    GameObject g = Instantiate(assemblyLinePrefab,
                        new Vector3(wObject.position.x, wObject.position.y, wObject.position.z),
                        Quaternion.Euler(new Vector3(wObject.rotation.x, wObject.rotation.y, wObject.rotation.z)));
                    g.GetComponent<BuildableObject>().inputOutputManager.GetComponent<NetworkObject>().WorldObject =
                        wObject;

                    worldObjects.TryAdd(wObject.id, g);
                }
                else if (wObject is SimpleFactoryServerLib.Objects.OreOutputMachine)
                {
                    GameObject g = Instantiate(oreOutputMachine,
                        new Vector3(wObject.position.x, wObject.position.y, wObject.position.z),
                        Quaternion.Euler(new Vector3(wObject.rotation.x, wObject.rotation.y, wObject.rotation.z)));
                    g.GetComponent<BuildableObject>().inputOutputManager.GetComponent<NetworkObject>().WorldObject =
                        wObject;

                    worldObjects.TryAdd(wObject.id, g);
                }
                else if (wObject is SimpleFactoryServerLib.Objects.Splitter)
                {
                    GameObject g = Instantiate(splitter,
                        new Vector3(wObject.position.x, wObject.position.y, wObject.position.z),
                        Quaternion.Euler(new Vector3(wObject.rotation.x, wObject.rotation.y, wObject.rotation.z)));
                    g.GetComponent<BuildableObject>().inputOutputManager.GetComponent<NetworkObject>().WorldObject =
                        wObject;

                    worldObjects.TryAdd(wObject.id, g);
                }
                else if (wObject is SimpleFactoryServerLib.Objects.Combiner)
                {
                    GameObject g = Instantiate(combiner,
                        new Vector3(wObject.position.x, wObject.position.y, wObject.position.z),
                        Quaternion.Euler(new Vector3(wObject.rotation.x, wObject.rotation.y, wObject.rotation.z)));
                    g.GetComponent<BuildableObject>().inputOutputManager.GetComponent<NetworkObject>().WorldObject =
                        wObject;

                    worldObjects.TryAdd(wObject.id, g);
                }
                else if (wObject is SimpleFactoryServerLib.Objects.Materials.Ore)
                {
                    Debug.Log("Amount: " + ((SimpleFactoryServerLib.Objects.Materials.Ore) wObject).Amount);
                    SimpleFactoryServerLib.Objects.Materials.Ore oreMessage =
                        (SimpleFactoryServerLib.Objects.Materials.Ore) wObject;
                    if (oreMessage.material.Equals(Materials.oil))
                    {
                        GameObject g = Instantiate(oil,
                            new Vector3(wObject.position.x, wObject.position.y, wObject.position.z),
                            Quaternion.Euler(new Vector3(wObject.rotation.x, wObject.rotation.y, wObject.rotation.z)));
                        g.GetComponent<Ore>().WorldObject = wObject;
                        worldObjects.TryAdd(wObject.id, g);
                    }
                    else if (oreMessage.material.Equals(Materials.coal) ||
                             oreMessage.material.Equals(Materials.copper) ||
                             oreMessage.material.Equals(Materials.stone))
                    {
                        GameObject g = Instantiate(ore,
                            new Vector3(wObject.position.x, wObject.position.y, wObject.position.z),
                            Quaternion.Euler(new Vector3(wObject.rotation.x, wObject.rotation.y, wObject.rotation.z)));
                        g.GetComponent<Ore>().WorldObject = wObject;
                        g.GetComponent<Ore>().updateColor();
                        worldObjects.TryAdd(wObject.id, g);
                    }
                }
                else if (wObject is ProcessingMachine)
                {
                    GameObject g = Instantiate(processingMachine,
                        new Vector3(wObject.position.x, wObject.position.y, wObject.position.z),
                        Quaternion.Euler(new Vector3(wObject.rotation.x, wObject.rotation.y, wObject.rotation.z)));
                    g.GetComponent<BuildableObject>().inputOutputManager.GetComponent<NetworkObject>().WorldObject =
                        wObject;

                    worldObjects.TryAdd(wObject.id, g);
                }
                else if (wObject is SimpleFactoryServerLib.Objects.Chest)
                {
                    GameObject g = Instantiate(chest,
                        new Vector3(wObject.position.x, wObject.position.y, wObject.position.z),
                        Quaternion.Euler(new Vector3(wObject.rotation.x, wObject.rotation.y, wObject.rotation.z)));
                    g.GetComponent<BuildableObject>().inputOutputManager.GetComponent<NetworkObject>().WorldObject =
                        wObject;

                    worldObjects.TryAdd(wObject.id, g);
                }
            }
            else // Update objects
            {
                if (wObject is SimpleFactoryServerLib.Objects.Materials.Ore)
                {
                    // Update ores
                    Debug.Log("Update Ore");
                    Debug.Log("Ore at " + wObject.position + " now has amount " + ((SimpleFactoryServerLib.Objects.Materials.Ore) wObject).Amount);
                    worldObjects[wObject.id].GetComponent<NetworkObject>().WorldObject = wObject;
                }
                else
                {
                    BuildableObject buildableObject;
                    if (worldObjects[wObject.id].TryGetComponent(out buildableObject))
                    {
                        NetworkObject networkObject;
                        if (buildableObject.inputOutputManager != null &&
                            buildableObject.inputOutputManager.TryGetComponent(out networkObject))
                        {
                            networkObject.WorldObject = wObject;
                            //TODO Activate correct child
                            Debug.Log("Update gameobject " + wObject);
                        }
                    }
                    else
                    {
                        Debug.Log("Object has no buildable object component. Object is typeof(" + wObject.GetType() +
                                  ")");
                    }

                    //Debug.Log("Before: " + wObject.before.Count + " id: " + wObject.id + " next: " + wObject.next + " pos: " + wObject.position + " rot: " + wObject.rotation + " storage" + wObject.storage + " maxStorage: " + wObject.maxStorage + " Buffer: " + wObject.bufferStorage + 
                    // " output" + wObject.outputPosition);
                }
            }
        }
    }

    private void setDisconnectedForMainThread()
    {
        _disconnected = true;
    }

    private void loadStartMenuScene()
    {
        SceneManager.LoadScene(0);
        SceneManager.UnloadSceneAsync(1);
    }

    public Vector3 convertPosToVector3(Position pos)
    {
        return new Vector3(pos.x, pos.y, pos.z);
    }

    public Position convertVec3ToPosition(Vector3 pos)
    {
        return new Position(pos.x, pos.y, pos.z);
    }


    public Quaternion convertRotToQuaternion(Rotation rot)
    {
        return Quaternion.Euler(new Vector3(rot.x, rot.y, rot.z));
    }


    public Rotation convertRotToQuaternion(Quaternion rot)
    {
        Vector3 ro = rot.eulerAngles;
        return new Rotation(ro.x, ro.y, ro.z);
    }
}