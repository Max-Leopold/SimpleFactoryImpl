using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInventoryUI : MonoBehaviour, IInventoryCallbacks
{
    public Inventory playerInventory;
    public InventoryManager playerInventoryManager;
    public GameObject inventorySlotPrefab;
    public int xStart, yStart;
    public float xSpace, ySpace;
    public int numberOfColumns;
    private List<GameObject> items = new List<GameObject>();

    private MouseItem mouseItem = new MouseItem();

    private void Start()
    {
        playerInventory.addCallback(this);
        CreateDisplay();
    }

    private void OnEnable()
    {
        OnInventoryChanged();
        playerInventory.addCallback(this);
    }

    private void OnDisable()
    {
        playerInventory.RemoveCallback(this);
    }

    void CreateDisplay()
    {
        for (int i = 0; i < playerInventory.slots.Length; i++)
        {
            GameObject obj = Instantiate(inventorySlotPrefab, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
            GameObject itemSlot = obj.transform.Find("ItemSlot").gameObject;
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
            AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnPointerEnter(obj, index); });
            AddEvent(obj, EventTriggerType.PointerExit, delegate { OnPointerExit(obj, index); });
            AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnPointerDragStart(obj, index); });
            AddEvent(obj, EventTriggerType.EndDrag, delegate { OnPointerDragEnd(obj, index); });
            AddEvent(obj, EventTriggerType.Drag, delegate { OnPointerDrag(obj, index); });
            items.Add(obj);
        }
    }


    void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        EventTrigger.Entry t = new EventTrigger.Entry();
        t.eventID = type;
        t.callback.AddListener(action);
        trigger.triggers.Add(t);
    }


    Vector3 GetPosition(int i)
    {
        return new Vector3(xStart + (xSpace * (i % numberOfColumns)), yStart + (-ySpace * (i / numberOfColumns)), 0);
    }

    public void OnPointerEnter(GameObject obj, int index)
    {
        mouseItem.hoverGameObject = obj;
        mouseItem.hoverItem = playerInventory.slots[index];
    }

    public void OnPointerExit(GameObject obj, int index)
    {
        mouseItem.hoverGameObject = null;
        mouseItem.hoverItem = null;
    }

    public void OnPointerDragStart(GameObject obj, int index)
    {
        mouseItem.mObject = new GameObject();
        mouseItem.item = playerInventory.slots[index];
        var rt = mouseItem.mObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);
        mouseItem.mObject.transform.SetParent(transform.parent);
        if(playerInventory.slots[index].material != null)
        {
            var img = mouseItem.mObject.AddComponent<Image>();
            img.sprite = playerInventory.slots[index].material.uiSprite;
            img.raycastTarget = false;
        }
    }

    public void OnPointerDragEnd(GameObject obj, int index)
    {
        if (mouseItem.hoverGameObject != null)
        {
            playerInventory.SwapItems(mouseItem.item, mouseItem.hoverItem);
        }
        else
        {
            Debug.Log(playerInventoryManager == null);
            playerInventoryManager.DropMaterial(mouseItem.item);
        }
        Destroy(mouseItem.mObject);
        mouseItem.item = null;
    }

    public void OnPointerDrag(GameObject obj, int index)
    {
        if(mouseItem.mObject != null)
        {
            mouseItem.mObject.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    public void OnInventoryChanged()
    {
        foreach (var item in items)
        {
            Destroy(item);
        }
        items.Clear();
        CreateDisplay();
    }
}

public class MouseItem
{
    public GameObject mObject;
    public InventorySlot item;
    public InventorySlot hoverItem;
    public GameObject hoverGameObject;
    public SlotType slotType;
    public SlotType hoverSlotType;
    public AbstractMaterial hoverMat;
    public AbstractMaterial mat;
    public int amount;
    public int index;
    public int hoverIndex;
}
