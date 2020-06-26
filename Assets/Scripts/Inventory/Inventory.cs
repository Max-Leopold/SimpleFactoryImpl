using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Inventory", menuName = "ScribtableObjects/Inventory")]
public class Inventory : ScriptableObject
{

    public InventorySlot[] slots = new InventorySlot[40];
    private List<IInventoryCallbacks> callbacks = new List<IInventoryCallbacks>();



    public void init(int size = 40)
    {
        slots = new InventorySlot[size];
        for (int i = 0; i < size; i++)
        {
            slots[i] = new InventorySlot(null);
        }
        callbacks = new List<IInventoryCallbacks>();
    }

    public void addCallback(IInventoryCallbacks callback)
    {
        callbacks.Add(callback);
    }

    public void RemoveCallback(IInventoryCallbacks callback)
    {
        if(callbacks.Contains(callback))
            callbacks.Remove(callback);
    }

    public void SwapItems(InventorySlot from, InventorySlot to)
    {
        if(from.material == to.material)    // Merge both slots
        {
            from.Amount = to.AddToSlot(from.Amount);
        }
        else   // Swap both slots
        {
            InventorySlot tempSlot = new InventorySlot(from.material);
            tempSlot.Amount = from.Amount;
            from.material = to.material;
            from.Amount = to.Amount;
            to.material = tempSlot.material;
            to.Amount = tempSlot.Amount;
        }
        NotifyCallbacks();
    }

    public void RemoveItem(InventorySlot slot)
    {
        slot.Amount = 0;    // This will set the material to null automatically
        NotifyCallbacks();
    }

    /// <summary>
    /// Adds an item to the inventory, if the item is not in the inventory a new slot will be used
    /// </summary>
    /// <param name="material">The material that should be added</param>
    /// <param name="amount">The amount of the specific material</param>
    /// <returns>Returns the amount of items that could <b>not</b> be added due to a full inventory</returns>
    public int AddItem(AbstractMaterial material, int amount)
    {
        if (amount <= 0)
            return amount;
        int remaining = amount;
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].material == null)
            {
                slots[i].Amount = 0;
                slots[i].material = material;
                remaining = slots[i].AddToSlot(remaining);
                if (remaining == 0)
                {
                    NotifyCallbacks();
                    return remaining;
                }
                else
                    continue;
            }
            else if(slots[i].material == material)
            {
                remaining = slots[i].AddToSlot(remaining);
                if (remaining == 0)
                {
                    NotifyCallbacks();
                    return remaining;
                }
            }
        }
        NotifyCallbacks();
        return remaining;
    }

    private void NotifyCallbacks()
    {
        foreach (var cb in callbacks)
        {
            cb.OnInventoryChanged();
        }
    }

    public bool IsEmpty()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].Amount > 0)
            {
                return false;
            }
        }

        return true;
    }

    public void RemoveItems(int amount, AbstractMaterial material)
    {
        int materialSlot = GetSlotOfMaterial(material);
        if (materialSlot != -1)
        {
            slots[materialSlot].Amount -= amount;
        }
    }

    public int GetFirstNonNullSlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].Amount != 0)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetSlotOfMaterial(AbstractMaterial material)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].material == material)
            {
                return i;
            }
        }

        return -1;
    }

}

public interface IInventoryCallbacks
{
    void OnInventoryChanged();
}