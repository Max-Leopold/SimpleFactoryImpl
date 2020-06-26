﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class InputOutputMachine : OutputMachine
{
    private static readonly int PULL_MATERIAL_RATE = 5;
    
    public List<InputMachine> internalInputMachines = new List<InputMachine>();
    
    protected Dictionary<AbstractMaterial, InputMachine> MaterialInputMachineMap = new Dictionary<AbstractMaterial, InputMachine>();
    //protected Dictionary<AbstractMaterial, int> MaterialStorageMap = new Dictionary<AbstractMaterial, int>();
    
    protected bool canPull = true;

    protected abstract void pullMaterial(InputMachine inputMachine);
    protected abstract void updateInputMachineMap();

    protected abstract void removeMaterial();

    protected void pullMaterials()
    {
        foreach (InputMachine inputMachine in internalInputMachines)
        {
            pullMaterial(inputMachine);
        }

        canPull = false;
        StartCoroutine(waitForPull());
    }

    protected IEnumerator waitForPull()
    {
        yield return new WaitForSeconds(1);
        canPull = true;
    }
}