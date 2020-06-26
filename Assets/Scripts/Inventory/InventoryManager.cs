using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class InventoryManager : MonoBehaviour
{

    public GameObject canvasManager;
    
    public Inventory inventory;
    public GameSettings gameSettings;
    public LayerMask machineLayerMask;
    private FirstPersonController fpsController;
    
    
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject machineInteractionUI;
    [SerializeField] private GameObject chestInteractionUI;

    [SerializeField] private GameObject activeUI;     // Stores the currently active UI (can be own inventory, machine, chest, etc...)

    private void Start()
    {
        fpsController = GetComponent<FirstPersonController>();

        GameObject canvasManagerInstance = Instantiate(canvasManager, new Vector3(0, 0, 0), Quaternion.identity);
        CanvasManager canvasManagerScript = canvasManagerInstance.GetComponent<CanvasManager>();

        inventoryUI = canvasManagerScript.inventoryUI;
        inventoryUI.transform.Find("Content").GetComponent<PlayerInventoryUI>().playerInventoryManager = this;
        machineInteractionUI = canvasManagerScript.machineUI;
        chestInteractionUI = canvasManagerScript.chestUI;
    }

    private void Update()
    {
        // Toggle the Inventory UI
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (activeUI == machineInteractionUI || activeUI == chestInteractionUI)
            {
                ToggleUI(false);
                return;
            }
            bool toggle = inventoryUI.activeSelf;
            if (activeUI != null)
            {
                ToggleUI(false);
            }
            activeUI = inventoryUI;
            ToggleUI(!toggle);
        }
        if(Input.GetKeyDown(KeyCode.Mouse1) && activeUI == null)
        {
            Debug.Log("Shooting raycast");
            // Ray cast to a machine
            Ray ray = new Ray(fpsController.m_Camera.transform.position, fpsController.m_Camera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, gameSettings.INTERACTION_DISTANCE, machineLayerMask))
            {
                BuildableObject buildableObject = hit.transform.gameObject.GetComponent<BuildableObject>();
                InputOutputManager inputOutputManager = buildableObject.inputOutputManager;
                NetworkObject networkObject = inputOutputManager.gameObject.GetComponent<NetworkObject>();
                WorldObject worldObject = networkObject.WorldObject;

                if (worldObject is SimpleFactoryServerLib.Objects.AssemblyLine)
                {
                    Debug.Log("Show AssemblyLine Inventory");
                } else if (worldObject is SimpleFactoryServerLib.Objects.ProcessingMachine)
                {
                    Debug.Log("Show ProcessingMachine Inventory");
                    activeUI = machineInteractionUI;
                    ToggleUI(true);
                    MachineInteractionUI uiController = activeUI.GetComponent<MachineInteractionUI>();
                    uiController.SetMachineType((ProcessingMachine) worldObject, networkObject);
                } else if (worldObject is SimpleFactoryServerLib.Objects.Splitter)
                {
                    Debug.Log("Show Splitter Inventory");
                } else if (worldObject is SimpleFactoryServerLib.Objects.Combiner)
                {
                    Debug.Log("Show Combiner Inventory");
                }else if (worldObject is SimpleFactoryServerLib.Objects.OreOutputMachine)
                {
                    Debug.Log("Show Ore Output Machines Inventory");
                    activeUI = machineInteractionUI;
                    ToggleUI(true);
                    MachineInteractionUI uiController = activeUI.GetComponent<MachineInteractionUI>();
                    uiController.SetMachineType((SimpleFactoryServerLib.Objects.OreOutputMachine) worldObject, networkObject);
                }else if (worldObject is SimpleFactoryServerLib.Objects.Chest)
                {
                    activeUI = chestInteractionUI;
                    ToggleUI(true);
                    ChestInteractionUI uiController = activeUI.GetComponent<ChestInteractionUI>();
                    uiController.SetChest((SimpleFactoryServerLib.Objects.Chest) worldObject, networkObject);
                }
                
                /*if (outputMachine is AssemblyMachine)
                {
                    activeUI = machineInteractionUI;
                    ToggleUI(true);
                    MachineInteractionUI uiController = activeUI.GetComponent<MachineInteractionUI>();
                    Debug.Log("InputOutPutMachine found");
                    uiController.SetMachineType(MachineType.ASSEMBLY_MACHINE, outputMachine);
                }
                else if(outputMachine is Chest)
                {
                    Debug.Log("Chest found");
                    activeUI = chestInteractionUI;
                    ToggleUI(true);
                    ChestInteractionUI chestUi = activeUI.GetComponent<ChestInteractionUI>();
                    chestUi.SetChest((Chest)outputMachine);
                }
                else if(outputMachine is OreOutputMachine)
                {
                    activeUI = machineInteractionUI;
                    ToggleUI(true);
                    MachineInteractionUI uiController = activeUI.GetComponent<MachineInteractionUI>();
                    Debug.Log("OreOutputMachine found");
                    uiController.SetMachineType(MachineType.OUTPUT_MACHINE, outputMachine);
                }*/
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape) && activeUI != null)
        {
            ToggleUI(false);
        }
    }

    private void ToggleUI(bool show)
    {
        activeUI.SetActive(show);
        gameSettings.uiOverlayVisible = show;
        fpsController.m_MouseLook.SetCursorLock(!show);
        fpsController.enabled = !show;
        if (!show)
            activeUI = null;
    }

    public void DropMaterial(InventorySlot slot)
    {
        if(slot.material != null)
        {
            if(slot.material.prefab != null)
            {
                NetworkController networkController =
                    GameObject.Find("NetworkController").GetComponent<NetworkController>();
                DroppedMaterialMessage dm = new DroppedMaterialMessage();
                dm.position = networkController.convertVec3ToPosition(transform.position + transform.forward * 2);
                dm.amount = slot.Amount;
                dm.material = gameSettings.convertUnityMaterialToServerMaterial(slot.material);
                networkController.sendToServer(dm);
                
                inventory.RemoveItem(slot);
                /*
                GameObject droppedMaterial = Instantiate(slot.material.prefab, transform.position + (transform.forward * 2), Quaternion.identity);
                MaterialManager mm = droppedMaterial.GetComponent<MaterialManager>();
                mm.amount = slot.Amount;
                mm.material = slot.material;
                */
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        MaterialManager material = other.gameObject.GetComponent<MaterialManager>();
        if (material)
        {
            material.amount = inventory.AddItem(material.material, material.amount);
            DroppedMaterialMessage dm = new DroppedMaterialMessage();
            dm.id = material.id;
            GameObject.Find("NetworkController").GetComponent<NetworkController>().sendToServer(dm);
        }
    }

    private void OnApplicationQuit()
    {
        // Clear the inventory
        // inventory.slots = new InventorySlot[inventory.slots.Length];
    }
}
