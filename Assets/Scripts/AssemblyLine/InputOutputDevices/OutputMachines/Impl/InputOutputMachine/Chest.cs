﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class Chest : InputOutputMachine
{

    private List<IInventoryCallbacks> callbacks = new List<IInventoryCallbacks>();

    private void Start()
    {
        //40 Material Stacks (same as inv.) - 1 (Because 1 Stack is saved in the InputMachine)
        //maxItemStorage = 39;
        updateInputMachineMap();
    }

    public void addCallback(IInventoryCallbacks callback)
    {
        Debug.Log("Add callback");
        Inventory.addCallback(callback);
    }

    public void removeCallback(IInventoryCallbacks callback)
    {
        Inventory.RemoveCallback(callback);
    }

    private void notifyCallbacks()
    {
        foreach (var cb in callbacks)
        {
            cb.OnInventoryChanged();
        }
    }

    private void Update()
    {
        if (canPull)
        {
            pullMaterials();
        }

        updateInputMachineMap();
        updateOutputMaterial();
    }

    public Inventory getInventory()
    {
        return Inventory;
    }

    protected override void pullMaterial(InputMachine inputMachine)
    {
        if (inputMachine.OutputMachine != null)
        {
            int inventoryOverflow = Inventory.AddItem(inputMachine.getMaterial(), 1);
            inputMachine.Inventory.slots[0].Amount -= 1 - inventoryOverflow;
            /*
            if (MaterialStorageMap.ContainsKey(inputMachine.getMaterial()))
            {
                //Check if there is space in the stack
                int amount = MaterialStorageMap[inputMachine.getMaterial()];
                decimal stacks = Math.Ceiling(amount / (decimal) inputMachine.getMaterial().maxNumsPerInventorySlot);
                decimal space = (stacks * inputMachine.getMaterial().maxNumsPerInventorySlot) - amount;

                if (space > 0)
                {
                    int maxAvailableItems = inputMachine.itemStorage;
                    int take = Math.Min(1, maxAvailableItems);

                    MaterialStorageMap[inputMachine.getMaterial()] += take;
                    inputMachine.itemStorage -= take;
                    inventory.AddItem(inputMachine.getMaterial(), take);
                }
                else
                {
                    if (calculateFreeStack() > 0)
                    {
                        int maxAvailableItems = inputMachine.itemStorage;
                        int take = Math.Min(1, maxAvailableItems);

                        itemStorage += 1;
                        MaterialStorageMap[inputMachine.getMaterial()] += take;
                        inputMachine.itemStorage -= take;
                        inventory.AddItem(inputMachine.getMaterial(), take);
                    }
                }
            }
            else
            {
                if (calculateFreeStack() > 0)
                {
                    MaterialStorageMap.Add(inputMachine.getMaterial(), 0);
                    int maxAvailableItems = inputMachine.itemStorage;
                    int take = Math.Min(1, maxAvailableItems);

                    itemStorage += 1;
                    MaterialStorageMap[inputMachine.getMaterial()] += take;
                    inputMachine.itemStorage -= take;
                    inventory.AddItem(inputMachine.getMaterial(), take);
                }
            }*/
        }
    }

    protected override void updateInputMachineMap()
    {
        foreach (InputMachine inputMachine in internalInputMachines)
        {
            if (inputMachine.getMaterial() != null)
            {
                if (!MaterialInputMachineMap.ContainsKey(inputMachine.getMaterial()))
                {
                    MaterialInputMachineMap.Add(inputMachine.getMaterial(), null);
                }
            }
        }
    }

    protected override void removeMaterial()
    {
        throw new NotImplementedException();
    }

    /*private int calculateFreeStack()
    {
        int usedStacks = 0;
        foreach (AbstractMaterial material in MaterialStorageMap.Keys)
        {
            int amount = MaterialStorageMap[material];
            usedStacks += (int) Math.Ceiling(amount / (decimal) material.maxNumsPerInventorySlot);
        }

        return maxItemStorage - usedStacks;
    }*/

    private void updateOutputMaterial()
    {
        int firstNonNull = Inventory.GetFirstNonNullSlot();
        if (firstNonNull != -1)
        {
            outputMaterial = Inventory.slots[firstNonNull].material;
        }
        else
        {
            outputMaterial = null;
        }
    }
}