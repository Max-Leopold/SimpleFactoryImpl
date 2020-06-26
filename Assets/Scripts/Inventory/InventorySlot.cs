using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class InventorySlot
{

    public AbstractMaterial material;
    [SerializeField]
    private int _amount;
    public int Amount
    {
        get { return _amount; }
        set
        {
            _amount = value;
            if(value <= 0)
            {
                material = null;
            }
        }
    }


    public InventorySlot(AbstractMaterial material)
    {
        this.material = material;
    }

    /// <summary>
    /// Adds an amount of a specific item to this slot
    /// </summary>
    /// <param name="add">The amount to be added</param>
    /// <returns>Returns the amount items that could <b>not</b> be added (Slot is full)</returns>
    public int AddToSlot(int add)
    {
        if (material == null)
            return add;
        this.Amount += add;
        int delta = this.Amount - material.maxNumsPerInventorySlot;
        if(delta > 0)
        {
            this.Amount -= delta;
            return delta;
        }
        return 0;
    }
}
