﻿using UnityEngine;
using System;

public class Combiner : OutputMachine
{
    private void Update()
    {
        pullMaterials();
    }

    private void pullMaterials()
    {
        //Only works if materials are the same currently. Subject to change
        if (materialsAreTheSame())
        {
            pullMaterial(externalInputMachines[0]);
            pullMaterial(externalInputMachines[1]);
        }
    }

    private void pullMaterial(GameObject inputMachineObj)
    {
        if (inputMachineObj.GetComponent<InputMachine>().Inventory.slots[0].Amount > 0)
        {
            int inventoryOverflow = Inventory.AddItem(outputMaterial, 1);
            inputMachineObj.GetComponent<InputMachine>().Inventory.slots[0].Amount -= 1 - inventoryOverflow;
        }
    }

    private bool materialsAreTheSame()
    {
        outputMaterial = externalInputMachines[0].GetComponent<InputMachine>().getMaterial();
        return outputMaterial == externalInputMachines[1].GetComponent<InputMachine>().getMaterial();
    }
}