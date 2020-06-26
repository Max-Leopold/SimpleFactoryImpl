﻿using UnityEngine;
using System;

public class Splitter : InputMachine
{
    public GameObject[] outputMachines;

    private bool pushLeft;

    private void Update()
    {
        if (canPull && _outputMachine != null)
        {
            pullMaterials();
            if (pushLeft)
            {
                pushMaterial(outputMachines[0]);
            }
            else
            {
                pushMaterial(outputMachines[1]);
            }

            canPull = false;
            StartCoroutine(wait());
        }

        updateOutputMaterial();
    }

    private void pushMaterial(GameObject outputMachine)
    {
        OutputMachine outputMachineSupplier = _outputMachine.GetComponent<OutputMachine>();
        OutputMachine pushOutputMachine = outputMachine.GetComponent<OutputMachine>();
        if (pushOutputMachine.outputMaterial != _outputMachine.GetComponent<OutputMachine>().outputMaterial)
        {
            pushOutputMachine.outputMaterial = _outputMachine.GetComponent<OutputMachine>().outputMaterial;

            //Remove all storage. Subject to change
            //Move all to inv
            pushOutputMachine.Inventory.RemoveItem(Inventory.slots[0]);
        }

        int inventoryOverflow = pushOutputMachine.Inventory.AddItem(outputMachineSupplier.outputMaterial, 1);
        outputMachineSupplier.Inventory.slots[0].Amount -= 1 - inventoryOverflow;

        pushLeft = !pushLeft;
    }
}