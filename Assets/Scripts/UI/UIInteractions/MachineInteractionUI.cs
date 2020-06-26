using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using SimpleFactoryServerLib.Objects.Recipe;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Material = SimpleFactoryServerLib.Objects.Materials.Material;

public class MachineInteractionUI : MonoBehaviour, IInventoryCallbacks, NetworkObjectListener
{
    public Inventory playerInventory;
    public InventoryManager playerInventoryManager;
    public GameObject inventorySlotPrefab;
    public Transform playerInventoryStartPos;
    public Transform machineInventoryStartPos;
    public float xSpace, ySpace;
    public int numberOfColumns;
    public GameSettings settings;
    public Dropdown dropDown;

    public MachineType machineType;
    
    private List<GameObject> items = new List<GameObject>();
    private List<GameObject> inputAndOutputItems = new List<GameObject>();
    private List<UiSlot> inAndOutputSlots = new List<UiSlot>();

    private MouseItem mouseItem = new MouseItem();
    private SingleOutputWorldObject _outputMachine;
    private bool initDone = false;
    private bool updateUI = false;

    private NetworkObject _networkObject;
    private InputOutputManager oreMachineInputOutput;
    private List<SimpleFactoryServerLib.Objects.Recipe.Recipe> recipes = new List<SimpleFactoryServerLib.Objects.Recipe.Recipe>();

    private void Awake()
    {
        // Add new recipes here
        recipes.Add(RecipeHolder.Cable);
        recipes.Add(RecipeHolder.Diamond);
        recipes.Add(RecipeHolder.Plastic);
        recipes.Add(RecipeHolder.CompressedCoal);
        recipes.Add(RecipeHolder.CopperIngots);
        recipes.Add(RecipeHolder.CopperWire);
    }

    private void Start()
    {
        
        playerInventory.addCallback(this);
        //CreateDisplay();
        dropDown.onValueChanged.AddListener(delegate {
            myDropDownValueChangedHandler();
        });
    }

    public void Init()
    {
        initDone = false;
        //UpdateUI();
    }

    private void myDropDownValueChangedHandler()
    {
        NetworkController networkController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        Debug.Log("Value: " + dropDown.value);
        if (_outputMachine is SimpleFactoryServerLib.Objects.OreOutputMachine)
        {
            SimpleFactoryServerLib.Objects.OreOutputMachine machine = (SimpleFactoryServerLib.Objects.OreOutputMachine) _outputMachine;
            AbstractMaterial unityMat = settings.oreMaterials[dropDown.value];
            machine.outputMaterial = new SimpleFactoryServerLib.Objects.Materials.Material(unityMat.name);
            GameObject.Find("NetworkController").GetComponent<NetworkController>().sendToServer(new InstantiateMessage(machine));
        }else if (_outputMachine is ProcessingMachine)
        {
            ProcessingMachine pMachine = (ProcessingMachine) _outputMachine;
            if (pMachine.outputStorage > 0)
            {
                Debug.Log("Storage is: " + pMachine.outputStorage);
                // Drop current output
                DroppedMaterialMessage dm = new DroppedMaterialMessage();
                dm.material = pMachine.recipe.output;
                dm.amount = pMachine.outputStorage;
                Position p = new Position(pMachine.position.x, pMachine.position.y + 1.5f, pMachine.position.z);
                dm.position = p;
                pMachine.outputStorage = 0;    // Set output to 0
                networkController.sendToServer(dm);

                List<Material> needed = new List<Material>();
                foreach (var inMat in recipes[dropDown.value].inputs)
                {
                    needed.Add(inMat.inputMaterial);    
                }

                List<Material> keys = pMachine.storage.Keys.ToList();
                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    if (!needed.Contains(keys[i]))
                    {
                        DroppedMaterialMessage d = new DroppedMaterialMessage();
                        d.material = keys[i];
                        d.amount = pMachine.storage[keys[i]];
                        d.position = p;

                        pMachine.storage.Remove(keys[i]);
                        networkController.sendToServer(d);
                    }
                }
            }
            pMachine.recipe = recipes[dropDown.value];
            networkController.sendToServer(new InstantiateMessage(pMachine));
        }
        /*
        if (_outputMachine is SimpleFactoryServerLib.Objects.OreOutputMachine)
        {
            SimpleFactoryServerLib.Objects.OreOutputMachine machine = (SimpleFactoryServerLib.Objects.OreOutputMachine) _outputMachine;
            
            machine.outputMaterial settings.recipes[dropDown.value]);
        }else if (machineType == MachineType.OUTPUT_MACHINE)
        {
            if(_outputMachine is OreOutputMachine)
                ((OreOutputMachine)_outputMachine).SetOutputMaterial(settings.oreMaterials[dropDown.value]);
            else
                _outputMachine.SetOutputMaterial(settings.oreMaterials[dropDown.value]);
        }
        */
    }

    private void OnDestroy()
    {
        dropDown.onValueChanged.RemoveAllListeners();
    }

    private void OnDisable()
    {
        if (_networkObject != null)
            _networkObject.networkObjectListener.Remove(this);
    }

    public void SetMachineType(SingleOutputWorldObject outputMachine, NetworkObject networkObject)
    {
        dropDown.options.Clear();
        this._outputMachine = outputMachine;
        this._networkObject = networkObject;
        this._networkObject.networkObjectListener.Add(this);

        if (outputMachine is SimpleFactoryServerLib.Objects.OreOutputMachine)
        {
            
            SimpleFactoryServerLib.Objects.OreOutputMachine machine =
                (SimpleFactoryServerLib.Objects.OreOutputMachine) outputMachine;
            
            if (machine.outputMaterial != null)
            {

                for (int i = 0; i < settings.oreMaterials.Count; i++)
                {
                    if (!(settings.oreMaterials[i] is OreMaterial))
                        continue;
                    dropDown.options.Add (new Dropdown.OptionData () { text = settings.oreMaterials[i].title });
                    if (machine.outputMaterial.name == settings.oreMaterials[i].title)
                    {
                        dropDown.captionText.text = settings.oreMaterials[i].title;
                        dropDown.value = i;
                    }
                }   
            }
            
            if (machine.outputMaterial == null)
            {
                dropDown.captionText.text = "Keine Erze in der Nähe gefunden";
                dropDown.value = -1;
            }
        }else if (outputMachine is ProcessingMachine)
        {
            ProcessingMachine machine = (ProcessingMachine) outputMachine;
            
            for (int i = 0; i < recipes.Count; i++)
            {
                dropDown.options.Add (new Dropdown.OptionData () { text = recipes[i].name });
                if (machine.recipe.name == recipes[i].name)
                {
                    dropDown.captionText.text = recipes[i].name;
                    dropDown.value = i;
                }
            }
            if (machine.recipe == null)
            {
                dropDown.captionText.text = "Kein Rezept ausgewählt";
                dropDown.value = -1;
            }
        }
        dropDown.RefreshShownValue();
        Init();
    }

    private void Update()
    {
        if (updateUI || (!initDone && _outputMachine != null))
        {
            if(mouseItem == null || mouseItem.mObject == null)
                UpdateUI();
        }
        /*
        else if(_outputMachine != null)
        {
            for (int i = 0; i < inAndOutputSlots.Count; i++)
            {
                if (inAndOutputSlots[i].img != null && inAndOutputSlots[i].text != null)
                {
                    if (inAndOutputSlots[i].isInputSlot && machineType == MachineType.ASSEMBLY_MACHINE)
                    {
                        InputOutputMachine inOutMachine = (InputOutputMachine)_outputMachine;
                        if (inOutMachine != null && inOutMachine.internalInputMachines != null && i < inOutMachine.internalInputMachines.Count)
                        {
                            AbstractMaterial mat = inOutMachine.internalInputMachines[i].GetComponent<InputMachine>().getMaterial();
                            if (inAndOutputSlots[i].material != mat)
                            {
                                if(mat == null)
                                    inAndOutputSlots[i].img.sprite = settings.InventoryEmptySlot;
                                else
                                    inAndOutputSlots[i].img.sprite = mat.uiSprite;
                            }
                            //Wenns nicht geht: Max hats kaputt gemacht
                            inAndOutputSlots[i].text.text = "" + inOutMachine.internalInputMachines[i].GetComponent<InputMachine>().Inventory.slots[0].Amount;
                        }
                    }
                    else if (!inAndOutputSlots[i].isInputSlot)
                    {
                        AbstractMaterial mat = _outputMachine.outputMaterial;
                        if (inAndOutputSlots[i].img.sprite != mat.uiSprite)
                        {
                            inAndOutputSlots[i].img.sprite = mat.uiSprite;
                        }
                        //Wenns nicht geht: Max hats kaputt gemacht
                        inAndOutputSlots[i].text.text = "" + _outputMachine.Inventory.slots[0].Amount;
                    }
                }
                else
                {
                    Debug.LogError("Img: " + inAndOutputSlots[i].img + " Text: " + inAndOutputSlots[i].text);
                }
            }
        }
        */
    }

    void CreateDisplay()
    {
        for (int i = 0; i < playerInventory.slots.Length; i++)
        {
            GameObject obj = Instantiate(inventorySlotPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i, playerInventoryStartPos);
            GameObject itemSlot = obj.transform.Find("ItemSlot").gameObject;
            OreMaterial unityMat = null;
            if(playerInventory.slots[i].material != null && playerInventory.slots[i].material is OreMaterial)
                unityMat = (OreMaterial)playerInventory.slots[i].material;
            
            if (playerInventory.slots[i].material != null)
            {
                itemSlot.GetComponent<Image>().sprite = playerInventory.slots[i].material.uiSprite;
                itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = "" + playerInventory.slots[i].Amount;
            }
            else
            {
                itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = "";
            }

            int index = i;
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnPointerEnter(obj, index, SlotType.PLAYER_INVENTORY, unityMat); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnPointerExit(obj, index, SlotType.PLAYER_INVENTORY, unityMat); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnPointerDragStart(obj, index, SlotType.PLAYER_INVENTORY, unityMat); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnPointerDragEnd(obj, index, SlotType.PLAYER_INVENTORY, unityMat); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnPointerDrag(obj, index, SlotType.PLAYER_INVENTORY, unityMat); });
            items.Add(obj);
        }

        if (_outputMachine != null && _outputMachine is SimpleFactoryServerLib.Objects.ProcessingMachine)
        {
            ProcessingMachine machine = (ProcessingMachine) _outputMachine;
            List<Material> storageMaterials = _outputMachine.storage.Keys.ToList();
            for (int i = 0; i < 4; i++)
            {
                // 4 normal storage slots and one output
                GameObject obj = Instantiate(inventorySlotPrefab, Vector3.zero, Quaternion.identity, transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i, machineInventoryStartPos);
                GameObject itemSlot = obj.transform.Find("ItemSlot").gameObject;
                AbstractMaterial unityMat = storageMaterials.Count > i ? settings.convertMaterialToUnityMaterial(storageMaterials[i]) : null;
                string text = "";
                if (unityMat != null)
                {
                    itemSlot.GetComponent<Image>().sprite = unityMat.uiSprite;
                    text = "" + _outputMachine.storage[storageMaterials[i]];
                }
                itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = text;
            
                int index = i;
                AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnPointerEnter(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                AddEvent(obj, EventTriggerType.PointerExit, delegate { OnPointerExit(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnPointerDragStart(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnPointerDragEnd(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnPointerDrag(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                inputAndOutputItems.Add(obj);
            
                inAndOutputSlots.Add(new UiSlot(itemSlot.GetComponent<Image>(), itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>(), unityMat, false));

            }
            // Show outputting material
            if (machine.recipe != null)
            {
                GameObject obj = Instantiate(inventorySlotPrefab, Vector3.zero, Quaternion.identity, transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(5, machineInventoryStartPos);
                GameObject itemSlot = obj.transform.Find("ItemSlot").gameObject;
                AbstractMaterial unityMat = settings.convertMaterialToUnityMaterial(machine.recipe.output);
                string text = "" + machine.outputStorage;
                if (unityMat != null)
                {
                    itemSlot.GetComponent<Image>().sprite = unityMat.uiSprite;
                }
                itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = text;
            
                int index = 5;
                AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnPointerEnter(obj, index, SlotType.MACHINE_OUTPUT, unityMat); });
                AddEvent(obj, EventTriggerType.PointerExit, delegate { OnPointerExit(obj, index, SlotType.MACHINE_OUTPUT, unityMat); });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnPointerDragStart(obj, index, SlotType.MACHINE_OUTPUT, unityMat); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnPointerDragEnd(obj, index, SlotType.MACHINE_OUTPUT, unityMat); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnPointerDrag(obj, index, SlotType.MACHINE_OUTPUT, unityMat); });
                inputAndOutputItems.Add(obj);
            
                inAndOutputSlots.Add(new UiSlot(itemSlot.GetComponent<Image>(), itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>(), unityMat, false));
            }
        }
        else
        {   // Used for Ore Output Machine
            // Current storage
            int pos = 0;
            foreach (var item in _outputMachine.storage)
            {
                GameObject obj = Instantiate(inventorySlotPrefab, Vector3.zero, Quaternion.identity, transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(pos, machineInventoryStartPos);
                GameObject itemSlot = obj.transform.Find("ItemSlot").gameObject;
                AbstractMaterial unityMat = settings.convertMaterialToUnityMaterial(item.Key);
                string text = "";
                if (unityMat != null)
                {
                    itemSlot.GetComponent<Image>().sprite = unityMat.uiSprite;
                    //Wenns nicht geht: Max hats kaputt gemacht
                    text = "" + item.Value;    
                }
                itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = text;
            
                int index = pos;
                AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnPointerEnter(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                AddEvent(obj, EventTriggerType.PointerExit, delegate { OnPointerExit(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnPointerDragStart(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnPointerDragEnd(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnPointerDrag(obj, index, SlotType.MACHINE_STORAGE, unityMat); });
                inputAndOutputItems.Add(obj);
            
                inAndOutputSlots.Add(new UiSlot(itemSlot.GetComponent<Image>(), itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>(), unityMat, false));
            
                pos++;
            }  
        }
        initDone = true;
    }


    void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        EventTrigger.Entry t = new EventTrigger.Entry();
        t.eventID = type;
        t.callback.AddListener(action);
        trigger.triggers.Add(t);
    }


    Vector3 GetPosition(int i, Transform transform)
    {
        return new Vector3(transform.localPosition.x + (xSpace * (i % numberOfColumns)), transform.localPosition.y + (-ySpace * (i / numberOfColumns)), 0);
    }

    public void OnPointerEnter(GameObject obj, int index, SlotType slotType, AbstractMaterial unityMat)
    {
        mouseItem.hoverGameObject = obj;
        mouseItem.hoverIndex = index;
        mouseItem.hoverMat = unityMat;
        mouseItem.hoverSlotType = slotType;
        if(slotType == SlotType.PLAYER_INVENTORY)
            mouseItem.hoverItem = playerInventory.slots[index];
    }

    public void OnPointerExit(GameObject obj, int index, SlotType slotType, AbstractMaterial unityMat)
    {
        mouseItem.hoverGameObject = null;
        mouseItem.hoverItem = null;
        mouseItem.hoverIndex = 0;
        mouseItem.hoverMat = null;
        mouseItem.hoverSlotType = SlotType.NONE;
    }

    public void OnPointerDragStart(GameObject obj, int index, SlotType slotType, AbstractMaterial unityMat)
    {
        mouseItem.mObject = new GameObject();
        var rt = mouseItem.mObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);
        mouseItem.mObject.transform.SetParent(transform.parent);
        mouseItem.slotType = slotType;
        mouseItem.mat = unityMat;
        mouseItem.index = index;

        AbstractMaterial material = unityMat;

        switch (slotType)
        {
            case SlotType.PLAYER_INVENTORY:
                mouseItem.item = playerInventory.slots[index];
                material = playerInventory.slots[index].material;
                mouseItem.amount = playerInventory.slots[index].Amount;
                break;
            case SlotType.MACHINE_STORAGE:
                Material mat = settings.convertUnityMaterialToServerMaterial(unityMat);
                mouseItem.amount = _outputMachine.storage.ContainsKey(mat) ? _outputMachine.storage[mat] : 0;
                // InputOutputMachine inOutMachine = (InputOutputMachine)_outputMachine;
                // material = inOutMachine.internalInputMachines[index].getMaterial();
                // //Wenns nicht geht: Max hats kaputt gemacht
                // mouseItem.amount = inOutMachine.internalInputMachines[index].Inventory.slots[0].Amount;
                break;
            case SlotType.MACHINE_OUTPUT:
                if (_outputMachine is ProcessingMachine)
                {
                    mouseItem.amount = ((ProcessingMachine) _outputMachine).outputStorage;
                }
                break;
        }

        if(material != null)
        {
            var img = mouseItem.mObject.AddComponent<Image>();
            img.sprite = material.uiSprite;
            img.raycastTarget = false;
        }
    }

    public void OnPointerDragEnd(GameObject obj, int index, SlotType slotType, AbstractMaterial unityMat)
    {        
        if (mouseItem.hoverGameObject != null)
        {
            Debug.Log("Drag end");
        
            // Player inventory swap
            if (mouseItem.slotType == SlotType.PLAYER_INVENTORY && mouseItem.hoverSlotType == SlotType.PLAYER_INVENTORY && mouseItem.item != null && mouseItem.hoverItem != null)
            {
                playerInventory.SwapItems(mouseItem.item, mouseItem.hoverItem);
            }
            else if (mouseItem.slotType == SlotType.PLAYER_INVENTORY && mouseItem.hoverSlotType == SlotType.MACHINE_STORAGE)
            {
                // Player => Output machine
                if (mouseItem.mat != null && mouseItem.amount > 0)
                {
                    Material mat = settings.convertUnityMaterialToServerMaterial(mouseItem.mat);
                    int delta = _outputMachine.tryAddToStorage(
                        new KeyValuePair<Material, int>(mat, mouseItem.amount));
                    int moved = mouseItem.amount - delta;
                    playerInventory.slots[mouseItem.index].Amount -= moved;
                    
                    InstantiateMessage instMessage = new InstantiateMessage(_outputMachine);
                    GameObject.Find("NetworkController").GetComponent<NetworkController>().sendToServer(instMessage);
                    // inOutMachine.internalInputMachines[mouseItem.index].Inventory.slots[0].Amount -= (mouseItem.amount - delta);
                }
            }else if(mouseItem.slotType == SlotType.MACHINE_STORAGE && mouseItem.hoverSlotType == SlotType.PLAYER_INVENTORY)
            {   // Machine storage => Player
                if (mouseItem.mat != null && mouseItem.amount > 0 && (playerInventory.slots[mouseItem.hoverIndex].material == null || mouseItem.mat == playerInventory.slots[mouseItem.hoverIndex].material))
                {
                    playerInventory.slots[mouseItem.hoverIndex].material = mouseItem.mat;
                    int delta = playerInventory.slots[mouseItem.hoverIndex].AddToSlot(mouseItem.amount);
                    //Wenns nicht geht: Max hats kaputt gemacht
                    Material mat = settings.convertUnityMaterialToServerMaterial(mouseItem.mat);
                    if (mat != null && _outputMachine.storage.ContainsKey(mat))
                    {
                        _outputMachine.storage[mat] -= mouseItem.amount - delta;
                        if (_outputMachine.storage[mat] <= 0)
                            _outputMachine.storage.Remove(mat);    // Remove item from dictionary (otherwise it's still shown in the UI)
                        InstantiateMessage instMessage = new InstantiateMessage(_outputMachine);
                        GameObject.Find("NetworkController").GetComponent<NetworkController>().sendToServer(instMessage);
                    }
                    // inOutMachine.internalInputMachines[mouseItem.index].Inventory.slots[0].Amount -= (mouseItem.amount - delta);
                }
            }
            else if(mouseItem.slotType == SlotType.MACHINE_OUTPUT && mouseItem.hoverSlotType == SlotType.PLAYER_INVENTORY)
            {
                Debug.Log("Output to player");
                Debug.Log("Amount: " + mouseItem.amount);
                Debug.Log("Mat: " + mouseItem.mat);
                Debug.Log("Player: " + playerInventory.slots[mouseItem.hoverIndex].material);
                // Ouput machine => Player
                if (mouseItem.amount > 0 && (playerInventory.slots[mouseItem.hoverIndex].material == null || playerInventory.slots[mouseItem.hoverIndex].material == mouseItem.mat))
                {
                    Debug.Log("Adding 1");
                    playerInventory.slots[mouseItem.hoverIndex].material = mouseItem.mat;
                    Debug.Log("Adding 2");
                    int delta = playerInventory.slots[mouseItem.hoverIndex].AddToSlot(mouseItem.amount);
                    Debug.Log("Adding 3 + " + delta);
                    //Wenns nicht geht: Max hats kaputt gemacht
                    Material mat = settings.convertUnityMaterialToServerMaterial(mouseItem.mat);
                    Debug.Log("Adding 4");
                    if (mat != null && _outputMachine.storage.ContainsKey(mat))
                    {
                        Debug.Log("Adding 5");
                        ((ProcessingMachine)_outputMachine).outputStorage -= mouseItem.amount - delta;
                        InstantiateMessage instMessage = new InstantiateMessage(_outputMachine);
                        GameObject.Find("NetworkController").GetComponent<NetworkController>().sendToServer(instMessage);
                    }
                    Debug.Log("Adding 6");
                }
            }
        }
        Destroy(mouseItem.mObject);
        mouseItem.item = null;
        OnInventoryChanged();
    }

    public void OnPointerDrag(GameObject obj, int index, SlotType slotType, AbstractMaterial unityMat)
    {
        if(mouseItem.mObject != null)
        {
            mouseItem.mObject.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    private void UpdateUI()
    {
        foreach (var item in items)
        {
            Destroy(item);
        }
        items.Clear();
        foreach (var item in inputAndOutputItems)
        {
            Destroy(item);
        }
        inputAndOutputItems.Clear();
        inAndOutputSlots.Clear();
        CreateDisplay();
        updateUI = false;
    }

    public void OnInventoryChanged()
    {
        // Sets the bool to true, because the UI needs to be updated on the main thread
        updateUI = true;
    }

    public void updateObject(WorldObject worldObject)
    {
        if (worldObject is SingleOutputWorldObject)
            this._outputMachine = (SingleOutputWorldObject) worldObject;
        updateUI = true;
    }
}

public struct UiSlot
{
    public Image img;
    public Text text;
    public AbstractMaterial material;
    public bool isInputSlot;

    public UiSlot(Image img, Text text, AbstractMaterial material, bool isInputSlot)
    {
        this.img = img;
        this.text = text;
        this.material = material;
        this.isInputSlot = isInputSlot;
    }
}

public enum MachineType
{
    OUTPUT_MACHINE,
    ASSEMBLY_MACHINE
}

public enum SlotType
{
    NONE,
    PLAYER_INVENTORY,
    MACHINE_STORAGE,
    MACHINE_OUTPUT,
    CHEST
}
