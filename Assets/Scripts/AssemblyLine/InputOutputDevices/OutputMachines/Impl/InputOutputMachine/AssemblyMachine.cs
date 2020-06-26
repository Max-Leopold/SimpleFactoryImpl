﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class AssemblyMachine : InputOutputMachine
{
    public Recipe recipe;

    private bool canBuild = true;

    private void Start()
    {
        setRecipe(recipe);
    }

    private void Update()
    {
        base.Update();
        updateInputMachineMap();

        if (canBuild)
        {
            buildRecipe();
        }
    }

    public void setRecipe(Recipe recipe)
    {
        this.recipe = recipe;

        updateRecipe();
    }

    private void updateRecipe()
    {
        SetOutputMaterial(recipe.output);

        MaterialInputMachineMap = new Dictionary<AbstractMaterial, InputMachine>();

        foreach (InputItems inputItems in recipe.inputs)
        {
            AbstractMaterial abstractMaterial = inputItems.inputMaterial;

            if (!MaterialInputMachineMap.ContainsKey(abstractMaterial))
            {
                MaterialInputMachineMap.Add(abstractMaterial, null);
            }
        }

        updateInputMachineMap();
    }

    IEnumerator waitForBuild()
    {
        yield return new WaitForSeconds(recipe.timeToCraft);
        canBuild = true;
    }

    private void buildRecipe()
    {
        if (enoughMaterials())
        {
            int inventoryOverflow = Inventory.AddItem(outputMaterial, 1);
            foreach (InputItems inputItem in recipe.inputs)
            {
                AbstractMaterial am = inputItem.inputMaterial;
                MaterialInputMachineMap[am].Inventory.slots[0].Amount -= inputItem.amount * (1 - inventoryOverflow);
            }

            canBuild = false;
            StartCoroutine(waitForBuild());
        }
    }

    private bool enoughMaterials()
    {
        foreach (InputItems inputItem in recipe.inputs)
        {
            AbstractMaterial am = inputItem.inputMaterial;
            if (MaterialInputMachineMap[am] == null || MaterialInputMachineMap[am].Inventory.slots[0].Amount < inputItem.amount)
            {
                return false;
            }
        }

        return true;
    }

    protected override void pullMaterial(InputMachine inputMachine)
    {
        throw new NotImplementedException();
    }

    protected override void updateInputMachineMap()
    {
        foreach (InputMachine inputMachine in internalInputMachines)
        {
            if (inputMachine.getMaterial() != null //&&
                //MaterialInputMachineMap.ContainsKey(inputMachine.getMaterial()) &&
                //MaterialInputMachineMap[inputMachine.getMaterial()] != inputMachine
            )
            {
                if (!MaterialInputMachineMap.ContainsKey(inputMachine.getMaterial()))
                {
                    removeInputMachine(inputMachine);
                    MaterialInputMachineMap.Add(inputMachine.getMaterial(), inputMachine);
                }
                else if (MaterialInputMachineMap.ContainsKey(inputMachine.getMaterial()))
                {
                    if (MaterialInputMachineMap[inputMachine.getMaterial()] != inputMachine)
                    {
                        removeInputMachine(inputMachine);
                        if (MaterialInputMachineMap[inputMachine.getMaterial()] == null)
                        {
                            MaterialInputMachineMap[inputMachine.getMaterial()] = inputMachine;
                        }
                    }
                }
            }
        }
    }

    protected override void removeMaterial()
    {
        throw new NotImplementedException();
    }

    private void removeInputMachine(InputMachine inputMachine)
    {
        foreach (AbstractMaterial am in MaterialInputMachineMap.Keys)
        {
            if (MaterialInputMachineMap[am] == inputMachine)
            {
                MaterialInputMachineMap[am] = null;
            }
        }
    }
}