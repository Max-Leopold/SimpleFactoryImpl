﻿using System;
using System.Collections;
using UnityEngine;


public class InputMachine : MonoBehaviour
{
    //public int itemStorage = 0;
    //public int maxItemStorage = 10; // Every input machine can store a specific amount of units

    protected bool canPull = true;

    [SerializeField] protected GameObject _inputAssemblyLine;
    [SerializeField] protected GameObject _outputMachine;
    [SerializeField] public AbstractMaterial _outputMaterial;

    public Inventory _inventory;

    private void Awake()
    {
        _inventory = ScriptableObject.CreateInstance(typeof(Inventory)) as Inventory;
        _inventory.init(1);
    }

    protected void Update()
    {
        if (canPull && _outputMachine != null)
        {
            pullMaterials();
            canPull = false;
            StartCoroutine(wait());
        }

        updateOutputMaterial();
    }

    protected void updateOutputMaterial()
    {
        if (_inventory.IsEmpty())
        {
            _outputMaterial = null;

            if (_outputMachine != null)
            {
                _outputMaterial = _outputMachine.GetComponent<OutputMachine>().outputMaterial;
            }
        }
        else
        {
            _outputMaterial = Inventory.slots[Inventory.GetFirstNonNullSlot()].material;
        }
    }

    protected IEnumerator wait()
    {
        yield return new WaitForSeconds(1);
        canPull = true;
    }

    protected void pullMaterials()
    {
        if (_outputMachine.GetComponent<OutputMachine>().outputMaterial == _outputMaterial)
        {
            int maxToTake = _outputMachine.GetComponent<OutputMachine>().Inventory.slots[0].Amount;
            //Change for better Assembly Lines
            int take = Math.Min(1, maxToTake);
            int inventoryOverflow = Inventory.AddItem(_outputMaterial, take);

            _outputMachine.GetComponent<OutputMachine>().Inventory
                .RemoveItems(take - inventoryOverflow, _outputMaterial);
        }
    }


    public GameObject InputAssemblyLine
    {
        get => _inputAssemblyLine;
        set => _inputAssemblyLine = value;
    }

    public GameObject OutputMachine
    {
        get => _outputMachine;
        set => _outputMachine = value;
    }

    public AbstractMaterial getMaterial()
    {
        updateOutputMaterial();
        return _outputMaterial;
    }

    public Inventory Inventory
    {
        get => _inventory;
        set => _inventory = value;
    }
}