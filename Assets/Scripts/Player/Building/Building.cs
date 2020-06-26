using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using SimpleFactoryServerLib.Objects.Recipe;
using UnityEngine;

public enum buildModes
{
    CONSECUTIVE,
    SINGLE,
    ORE_MACHINE,
    SPLITTER,
    COMBINER,
    MACHINE,
    CHEST,
    DELETE,
    FOLLOW,
    NOTHING
}

public class Building : MonoBehaviour
{
    private NetworkController networkController;

    public GameObject hand;

    public GameObject assemblyLinePreview;
    public GameObject assembllyLineStandard;

    public GameObject assemblyLineWithGrid;
    public GameObject splitterWithGrid;
    public GameObject combinerWithGrid;
    public GameObject oreMachineWithGrid;
    public GameObject processingMachineWithGrid;
    public GameObject chestWithGrid;
    public GameObject deleteWithGrid;

    public Material transparentAssemblyLineMaterial;
    public Material nonTransparentAssemblyLineMaterial;

    public LayerMask builableObjectLayer;
    public LayerMask layerOfOutput;
    public LayerMask layerOfOutputAndAssemblyLine;

    public Camera cam;

    public bool enableDebugOutput = true;

    //--------------------------------------------------------------------------

    [SerializeField] private bool _startingPositionSet;
    private Vector3 _startingPosition = Vector3.zero;
    private AStar _aStarAlgorithm;
    private AStarNode _aStarEndPosition = new AStarNode(Vector3.zero, Vector3.zero);
    private LayerMask _everythingExceptAssemblyLineLayer;
    private LayerMask _everythingExceptAssemblyLineAndOutput;
    private GameObject _objInHand;

    private buildModes _buildModes { get; set; }

    private void Start()
    {
        _everythingExceptAssemblyLineLayer = ~ builableObjectLayer;
        _everythingExceptAssemblyLineAndOutput = ~ layerOfOutputAndAssemblyLine;
        _aStarAlgorithm = new AStar(_everythingExceptAssemblyLineAndOutput);
        _buildModes = buildModes.NOTHING;
        networkController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            hand.transform.Rotate(Vector3.up, 90);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _buildModes = buildModes.SINGLE;
            showHandPositionPreview(assemblyLineWithGrid);
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _buildModes = buildModes.CONSECUTIVE;
            showHandPositionPreview(assemblyLineWithGrid);
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _buildModes = buildModes.ORE_MACHINE;
            showHandPositionPreview(oreMachineWithGrid);
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _buildModes = buildModes.SPLITTER;
            showHandPositionPreview(splitterWithGrid);
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _buildModes = buildModes.COMBINER;
            showHandPositionPreview(combinerWithGrid);
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            _buildModes = buildModes.MACHINE;
            showHandPositionPreview(processingMachineWithGrid);
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            _buildModes = buildModes.CHEST;
            showHandPositionPreview(chestWithGrid);
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _buildModes = buildModes.DELETE;
            showHandPositionPreview(deleteWithGrid);
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            _buildModes = buildModes.NOTHING;
            deleteHandPositionPreview();
            if (enableDebugOutput)
                Debug.Log("Building - Buildmode " + _buildModes + " set");
        }

        if (_buildModes != buildModes.NOTHING
            /*CustomGridUtil.validatePosition(hand.transform.position, _everythingExceptAssemblyLineLayer)*/)
        {
            if (_buildModes == buildModes.CONSECUTIVE)
            {
                buildConsecutive();
            }
            else if (_buildModes == buildModes.SINGLE)
            {
                buildSingle();
            }
            else if (_buildModes == buildModes.ORE_MACHINE)
            {
                buildOreOutput();
            }
            else if (_buildModes == buildModes.SPLITTER)
            {
                buildSplitter();
            }
            else if (_buildModes == buildModes.COMBINER)
            {
                buildCombiner();
            }
            else if (_buildModes == buildModes.MACHINE)
            {
                buildProcessingMachine();
            }
            else if (_buildModes == buildModes.CHEST)
            {
                buildChest();
            }
            else if (_buildModes == buildModes.DELETE)
            {
                deleteObject();
            }
        }
    }

    private void deleteObject()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), builableObjectLayer))
        {
            Collider[] hitColliders = Physics.OverlapBox(
                CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), new Vector3(
                    StaticGameVariables.ASSEMBLY_LINE_SIZE / 3, StaticGameVariables.ASSEMBLY_LINE_SIZE / 3,
                    StaticGameVariables.ASSEMBLY_LINE_SIZE / 3),
                Quaternion.identity, builableObjectLayer);
            
            for (int i = hitColliders.Length - 1; i >= 0; i--)
            {
                NetworkObject networkObject = hitColliders[i].gameObject.GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.WorldObject != null)
                {
                    DeleteMessage deleteMessage = new DeleteMessage(networkObject.WorldObject);
                    networkController.sendToServer(deleteMessage);
                    Debug.Log("Send to server is called " + deleteMessage);
                }
            }
        }
    }

    private void buildChest()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), builableObjectLayer))
        {
            SimpleFactoryServerLib.Objects.Chest chest =
                new SimpleFactoryServerLib.Objects.Chest(Tier.TIER_1);
            chest.position = NetworkUtil.convertVector3ToPosition(CustomGridUtil.getTruePos(
                hand.transform.position,
                StaticGameVariables.GRIDSIZE));
            chest.rotation =
                NetworkUtil.convertQuaternionToPosition(CustomGridUtil.getTrueRot(hand.transform.rotation, 0, 0, 0));
            InstantiateMessage instantiateMessage = new InstantiateMessage(chest);
            networkController.sendToServer(instantiateMessage);

            Debug.Log("Send to server is called " + instantiateMessage);
        }
    }

    private void buildProcessingMachine()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), builableObjectLayer))
        {
            ProcessingMachine processingMachine =
                new ProcessingMachine(RecipeHolder.CopperIngots, 5, Tier.TIER_1);
            processingMachine.position = NetworkUtil.convertVector3ToPosition(CustomGridUtil.getTruePos(
                hand.transform.position,
                StaticGameVariables.GRIDSIZE));
            processingMachine.rotation =
                NetworkUtil.convertQuaternionToPosition(CustomGridUtil.getTrueRot(hand.transform.rotation, 0, 0, 0));
            InstantiateMessage instantiateMessage = new InstantiateMessage(processingMachine);
            networkController.sendToServer(instantiateMessage);

            Debug.Log("Send to server is called " + instantiateMessage);
        }
    }

    private void buildCombiner()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), builableObjectLayer))
        {
            SimpleFactoryServerLib.Objects.Combiner combiner = new SimpleFactoryServerLib.Objects.Combiner(Tier.TIER_1);
            combiner.position = NetworkUtil.convertVector3ToPosition(CustomGridUtil.getTruePos(
                hand.transform.position,
                StaticGameVariables.GRIDSIZE));
            combiner.rotation =
                NetworkUtil.convertQuaternionToPosition(CustomGridUtil.getTrueRot(hand.transform.rotation, 0, 0, 0));
            InstantiateMessage instantiateMessage = new InstantiateMessage(combiner);
            networkController.sendToServer(instantiateMessage);

            Debug.Log("Send to server is called " + instantiateMessage);
        }
    }

    private void buildSplitter()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), builableObjectLayer))
        {
            SimpleFactoryServerLib.Objects.Splitter splitter =
                new SimpleFactoryServerLib.Objects.Splitter(Tier.TIER_1);
            splitter.position =
                NetworkUtil.convertVector3ToPosition(CustomGridUtil.getTruePos(hand.transform,
                    StaticGameVariables.GRIDSIZE, 0, 0));
            splitter.rotation =
                NetworkUtil.convertQuaternionToPosition(CustomGridUtil.getTrueRot(hand.transform, 0, 0, 0));
            InstantiateMessage instantiateMessage = new InstantiateMessage(splitter);
            networkController.sendToServer(instantiateMessage);
            Debug.Log("Send To Server is called");
        }
    }

    private void buildOreOutput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), builableObjectLayer))
        {
            SimpleFactoryServerLib.Objects.OreOutputMachine oreOutputMachine =
                new SimpleFactoryServerLib.Objects.OreOutputMachine(Tier.TIER_1);
            oreOutputMachine.position =
                NetworkUtil.convertVector3ToPosition(CustomGridUtil.getTruePos(hand.transform,
                    StaticGameVariables.GRIDSIZE, 0, 0));
            oreOutputMachine.rotation =
                NetworkUtil.convertQuaternionToPosition(CustomGridUtil.getTrueRot(hand.transform, 0, 0, 0));
            InstantiateMessage instantiateMessage = new InstantiateMessage(oreOutputMachine);
            networkController.sendToServer(instantiateMessage);
            Debug.Log("Send To Server is called");
        }
    }

    private void buildSingle()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), builableObjectLayer))
        {
            SimpleFactoryServerLib.Objects.AssemblyLine assemblyLine =
                new SimpleFactoryServerLib.Objects.AssemblyLine(AssemblyLineEnum.EmptyAssemblyLine, Tier.TIER_1);
            assemblyLine.position =
                NetworkUtil.convertVector3ToPosition(CustomGridUtil.getTruePos(hand.transform,
                    StaticGameVariables.GRIDSIZE, 0, 0));
            assemblyLine.rotation =
                NetworkUtil.convertQuaternionToPosition(CustomGridUtil.getTrueRot(hand.transform, 0, 0, 0));
            InstantiateMessage instantiateMessage = new InstantiateMessage(assemblyLine);
            networkController.sendToServer(instantiateMessage);
            Debug.Log("Send To Server is called");
        }
    }

    private void buildConsecutive()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !_startingPositionSet && CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE), builableObjectLayer))
        {
            _startingPosition = CustomGridUtil.getTruePos(hand.transform, StaticGameVariables.GRIDSIZE, 0, 0);
            _aStarEndPosition = _aStarAlgorithm.calculatePath(_startingPosition,
                CustomGridUtil.getTruePos(hand.transform, StaticGameVariables.GRIDSIZE, 0, 0));

            _startingPositionSet = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0) && _startingPositionSet && CustomGridUtil.validatePosition(
            CustomGridUtil.getTruePos(hand.transform.position, StaticGameVariables.GRIDSIZE),
            builableObjectLayer))
        {
            //Build path
            _aStarEndPosition = _aStarAlgorithm.calculatePath(
                CustomGridUtil.getTruePos(hand.transform, StaticGameVariables.GRIDSIZE, 0, 0), _startingPosition);

            //AssemblyLine assemblyLine =
            //Instantiate(assembllyLineStandard, _aStarEndPosition.getPos(), Quaternion.identity).GetComponent<AssemblyLine>();

            SimpleFactoryServerLib.Objects.AssemblyLine assemblyLine =
                new SimpleFactoryServerLib.Objects.AssemblyLine(AssemblyLineEnum.EmptyAssemblyLine, Tier.TIER_1);
            assemblyLine.position =
                NetworkUtil.convertVector3ToPosition(_aStarEndPosition.getPos());
            assemblyLine.rotation = NetworkUtil.convertQuaternionToPosition(Quaternion.identity);
            assemblyLine.forceScan = true;
            if (_aStarEndPosition.getPredecessor() != null)
            {
                assemblyLine.outputPosition =
                    NetworkUtil.convertVector3ToPosition(_aStarEndPosition.getPredecessor().getPos());
            }

            Position lastPos = assemblyLine.position;

            InstantiateMessage instantiateMessage = new InstantiateMessage(assemblyLine);
            networkController.sendToServer(instantiateMessage);
            Debug.Log("Send To Server is called");

            while (_aStarEndPosition.getPredecessor() != null)
            {
                _aStarEndPosition = _aStarEndPosition.getPredecessor();
                assemblyLine = new SimpleFactoryServerLib.Objects.AssemblyLine(AssemblyLineEnum.EmptyAssemblyLine, Tier.TIER_1);
                assemblyLine.position = NetworkUtil.convertVector3ToPosition(_aStarEndPosition.getPos());
                assemblyLine.rotation = NetworkUtil.convertQuaternionToPosition(Quaternion.identity);
                if (_aStarEndPosition.getPredecessor() != null)
                {
                    assemblyLine.outputPosition =
                        NetworkUtil.convertVector3ToPosition(_aStarEndPosition.getPredecessor().getPos());
                }

                assemblyLine.forceScan = true;
                instantiateMessage = new InstantiateMessage(assemblyLine);
                networkController.sendToServer(instantiateMessage);
                Debug.Log("Send To Server is called");
            }

            _startingPositionSet = false;
        }
    }

    private void deleteHandPositionPreview()
    {
        Destroy(_objInHand);
    }

    private void showHandPositionPreview(GameObject previewPrefab)
    {
        GameObject toDestroy = _objInHand;
        _objInHand = null;
        Destroy(toDestroy);
        if (_objInHand == null)
        {
            _objInHand = Instantiate(previewPrefab, hand.transform);
        }
    }
}