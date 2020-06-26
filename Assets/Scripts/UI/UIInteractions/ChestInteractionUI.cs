using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Material = SimpleFactoryServerLib.Objects.Materials.Material;

public class ChestInteractionUI : MonoBehaviour, IInventoryCallbacks, NetworkObjectListener
{
    private SimpleFactoryServerLib.Objects.Chest chest;
    public Inventory playerInventory;
    public Transform playerInventoryStart, chestInventoryStart;
    public float xSpace, ySpace;
    public int numberOfColumns;
    public GameObject inventorySlotPrefab;
    public NetworkObject networkObject;
    public GameSettings settings;
    
    private MouseItem mouseItem = new MouseItem();
    private List<GameObject> playerSlots = new List<GameObject>();
    private List<GameObject> chestSlots = new List<GameObject>();
    private GameObject playerContent, chestContent;
    private bool updateUi = false;


    private void Start()
    {
        playerContent = transform.Find("PlayerContent").gameObject;
        chestContent = transform.Find("ChestContent").gameObject;
        CreateDisplay();
        playerInventory.addCallback(this);
    }

    private void OnDisable()
    {
        networkObject.networkObjectListener.Remove(this);
    }

    private void Update()
    {
        if (updateUi && mouseItem.mObject == null)
        {
            updateUi = false;
            Debug.LogError("Updating ui");
            OnInventoryChanged();
        }
    }

    private void CreateDisplay()
    {
        if (playerContent != null)
        {

            for (int i = 0; i < playerInventory.slots.Length; i++)
            {
                GameObject obj = Instantiate(inventorySlotPrefab, Vector3.zero, Quaternion.identity, playerContent.transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i, playerInventoryStart);
                GameObject itemSlot = obj.transform.Find("ItemSlot").gameObject;
                AbstractMaterial material = null;
                if (playerInventory.slots[i].material != null)
                {
                    material = playerInventory.slots[i].material;
                    itemSlot.GetComponent<Image>().sprite = playerInventory.slots[i].material.uiSprite;
                    itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = "" + playerInventory.slots[i].Amount;
                }
                else
                {
                    itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = "";
                }

                int index = i;
                AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnPointerEnter(obj, index, SlotType.PLAYER_INVENTORY, material); });
                AddEvent(obj, EventTriggerType.PointerExit, delegate { OnPointerExit(obj, index, SlotType.PLAYER_INVENTORY, material); });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnPointerDragStart(obj, index, SlotType.PLAYER_INVENTORY, material); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnPointerDragEnd(obj, index, SlotType.PLAYER_INVENTORY, material); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnPointerDrag(obj, index, SlotType.PLAYER_INVENTORY, material); });
                playerSlots.Add(obj);
            }
        }

        if(chest != null && chestContent != null)
        {
            // Copy the map to prevent errors, if the chest edits the map at the same moment
            Dictionary<Material, int> storage = chest.storage;
            List<Material> mats = storage.Keys.ToList();
            for (int i = 0; i < 30; i++)
            {
                GameObject obj = Instantiate(inventorySlotPrefab, Vector3.zero, Quaternion.identity, chestContent.transform);
                obj.GetComponent<RectTransform>().localPosition = GetPosition(i, chestInventoryStart);
                GameObject itemSlot = obj.transform.Find("ItemSlot").gameObject;
                AbstractMaterial unityMat = null;
                if (mats.Count > i && mats[i] != null)
                {
                    unityMat = settings.convertMaterialToUnityMaterial(mats[i]);
                    itemSlot.GetComponent<Image>().sprite = unityMat.uiSprite;
                    itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = "" + storage[mats[i]];
                }
                else
                {
                    itemSlot.transform.Find("Amount").gameObject.GetComponent<Text>().text = "";
                }
                int index = i;
                AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnPointerEnter(obj, index, SlotType.CHEST, unityMat); });
                AddEvent(obj, EventTriggerType.PointerExit, delegate { OnPointerExit(obj, index, SlotType.CHEST, unityMat); });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnPointerDragStart(obj, index, SlotType.CHEST, unityMat); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnPointerDragEnd(obj, index, SlotType.CHEST, unityMat); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnPointerDrag(obj, index, SlotType.CHEST, unityMat); });
                chestSlots.Add(obj);
            }
        }
    }

    Vector3 GetPosition(int i, Transform transform)
    {
        return new Vector3(transform.localPosition.x + (xSpace * (i % numberOfColumns)), transform.localPosition.y + (-ySpace * (i / numberOfColumns)), 0);
    }

    void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        EventTrigger.Entry t = new EventTrigger.Entry();
        t.eventID = type;
        t.callback.AddListener(action);
        trigger.triggers.Add(t);
    }


    public void SetChest(SimpleFactoryServerLib.Objects.Chest chest, NetworkObject networkObject)
    {
        this.chest = chest;
        this.networkObject = networkObject;
        this.networkObject.networkObjectListener.Add(this);
        OnInventoryChanged();
    }


    public void OnPointerEnter(GameObject obj, int index, SlotType slotType, AbstractMaterial mat)
    {
        mouseItem.hoverGameObject = obj;
        mouseItem.hoverIndex = index;
        mouseItem.hoverMat = mat;
        mouseItem.hoverSlotType = slotType;
        if (slotType == SlotType.PLAYER_INVENTORY)
            mouseItem.hoverItem = playerInventory.slots[index];
    }

    public void OnPointerExit(GameObject obj, int index, SlotType slotType, AbstractMaterial mat)
    {
        mouseItem.hoverGameObject = null;
        mouseItem.hoverItem = null;
        mouseItem.hoverIndex = 0;
        mouseItem.hoverMat = null;
        mouseItem.hoverSlotType = SlotType.NONE;
    }

    public void OnPointerDragStart(GameObject obj, int index, SlotType slotType, AbstractMaterial mat)
    {
        mouseItem.mObject = new GameObject();
        var rt = mouseItem.mObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);
        mouseItem.mObject.transform.SetParent(transform.parent);
        mouseItem.slotType = slotType;
        mouseItem.mat = mat;
        mouseItem.index = index;


        switch (slotType)
        {
            case SlotType.PLAYER_INVENTORY:
                mouseItem.item = playerInventory.slots[index];
                mouseItem.amount = playerInventory.slots[index].Amount;
                break;
            case SlotType.CHEST:
                Material m = settings.convertUnityMaterialToServerMaterial(mat);
                if (m != null && chest.storage.ContainsKey(m))
                {
                    mouseItem.amount = chest.storage[m];
                }
                else
                {
                    mouseItem.amount = 0;
                }
                break;
        }

        if (mat != null)
        {
            var img = mouseItem.mObject.AddComponent<Image>();
            img.sprite = mat.uiSprite;
            img.raycastTarget = false;
        }
    }

    public void OnPointerDragEnd(GameObject obj, int index, SlotType slotType, AbstractMaterial mat)
    {
        if (mouseItem.hoverGameObject != null)
        {
            // Player inventory swap
            if (mouseItem.slotType == SlotType.PLAYER_INVENTORY && mouseItem.hoverSlotType == SlotType.PLAYER_INVENTORY && mouseItem.item != null && mouseItem.hoverItem != null)
            {
                playerInventory.SwapItems(mouseItem.item, mouseItem.hoverItem);
            }
            else if (mouseItem.slotType == SlotType.PLAYER_INVENTORY && mouseItem.hoverSlotType == SlotType.CHEST)
            {
                // Player => Chest
                if (mouseItem.mat != null && mouseItem.amount > 0)
                {
                    Material _mat = settings.convertUnityMaterialToServerMaterial(mouseItem.mat);
                    int delta = chest.tryAddToStorage(
                        new KeyValuePair<Material, int>(_mat, mouseItem.amount));
                    int moved = mouseItem.amount - delta;
                    playerInventory.slots[mouseItem.index].Amount -= moved;
                    
                    InstantiateMessage instMessage = new InstantiateMessage(chest);
                    GameObject.Find("NetworkController").GetComponent<NetworkController>().sendToServer(instMessage);
                }
            }
            else if (mouseItem.slotType == SlotType.CHEST && mouseItem.hoverSlotType == SlotType.PLAYER_INVENTORY)
            {

                // Chest => Player
                if (mouseItem.mat != null && mouseItem.amount > 0 && (mouseItem.mat == playerInventory.slots[mouseItem.hoverIndex].material || playerInventory.slots[mouseItem.hoverIndex].material == null))
                {
                    playerInventory.slots[mouseItem.hoverIndex].material = mouseItem.mat;
                    int delta = playerInventory.slots[mouseItem.hoverIndex].AddToSlot(mouseItem.amount);
                    //Wenns nicht geht: Max hats kaputt gemacht
                    Material _mat = settings.convertUnityMaterialToServerMaterial(mouseItem.mat);
                    if (mat != null && chest.storage.ContainsKey(_mat))
                    {
                        chest.storage[_mat] -= mouseItem.amount - delta;
                        if (chest.storage[_mat] <= 0)
                            chest.storage.Remove(_mat);    // Remove item from dictionary (otherwise it's still shown in the UI)
                        InstantiateMessage instMessage = new InstantiateMessage(chest);
                        GameObject.Find("NetworkController").GetComponent<NetworkController>().sendToServer(instMessage);
                    }
                    // inOutMachine.internalInputMachines[mouseItem.index].Inventory.slots[0].Amount -= (mouseItem.amount - delta);
                }
            }
        }
        Destroy(mouseItem.mObject);
        mouseItem.item = null;
        mouseItem.mObject = null;
        mouseItem.mat = null;
        StartCoroutine(waitForInventoryUpdate());
    }

    public void OnPointerDrag(GameObject obj, int index, SlotType slotType, AbstractMaterial mat)
    {
        if (mouseItem.mObject != null)
        {
            mouseItem.mObject.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }


    private IEnumerator waitForInventoryUpdate()
    {
        yield return new WaitForSeconds(.1f);
        OnInventoryChanged();
    }

    public void OnInventoryChanged()
    {
        if (mouseItem.mObject == null)
        {
            for (int i = 0; i < playerSlots.Count; i++)
            {
                Destroy(playerSlots[i]);
            }
            playerSlots.Clear();
            for (int i = 0; i < chestSlots.Count; i++)
            {
                Destroy(chestSlots[i]);
            }
            chestSlots.Clear();
            CreateDisplay();
        }
    }

    public void updateObject(WorldObject worldObject)
    {
        if (worldObject is SimpleFactoryServerLib.Objects.Chest)
            this.chest = (SimpleFactoryServerLib.Objects.Chest) worldObject;
        updateUi = true;
    }
}
