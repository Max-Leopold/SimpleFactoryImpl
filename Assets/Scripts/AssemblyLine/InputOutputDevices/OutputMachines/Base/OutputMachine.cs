﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class OutputMachine : MonoBehaviour
{
    
    public List<GameObject> externalInputMachines = new List<GameObject>();
    
    [SerializeField] private GameObject _outputAssemblyLine;
    [SerializeField] protected GameSettings _gameSettings;
    [SerializeField] protected OutputMachineTier _tier = OutputMachineTier.TIER1;
    [SerializeField] protected SpriteRenderer previewRenderer;
    [SerializeField] private GameSettings settings;

    public AbstractMaterial outputMaterial;
    
    //public int itemStorage = 0;
    //public int maxItemStorage = 10;    // Every output machine can store a specific amount of units
    
    public Inventory inventory;

    private void Awake()
    {
        inventory = ScriptableObject.CreateInstance(typeof(Inventory)) as Inventory;
        inventory.init(1);
    }
    
    
    public GameObject OutputAssemblyLine
    {
        get => _outputAssemblyLine;
        set => _outputAssemblyLine = value;
    }

    public Inventory Inventory
    {
        get => inventory;
        set => inventory = value;
    }

    private void Start()
    {
        if (previewRenderer)
        {
            if (outputMaterial != null)
            {
                previewRenderer.sprite = outputMaterial.uiSprite;
            }
            else
            {
                previewRenderer.sprite = settings.InventoryEmptySlot;
            }
        }
    }

    protected void Update()
    {
        if(previewRenderer != null)
            previewRenderer.transform.Rotate(new Vector3(0, 1.5f, 0));
    }

    public void addInputMachine(GameObject inputMachine)
    {
        externalInputMachines.Add(inputMachine);
        inputMachine.GetComponent<InputMachine>().OutputMachine = gameObject;
        
    }

    public void removeInputMachine(GameObject inputMachine)
    {
        inputMachine.GetComponent<InputMachine>().OutputMachine = null;
        externalInputMachines.Remove(inputMachine);
    }

    public void SetOutputMaterial(AbstractMaterial material)
    {
        outputMaterial = material;
        if(previewRenderer != null)
        {
            if (outputMaterial != null)
            {
                previewRenderer.sprite = outputMaterial.uiSprite;
            }
            else
            {
                previewRenderer.sprite = settings.InventoryEmptySlot;
            }
        }
    }
    
    public void SetTierMode(OutputMachineTier tier)
    {
        _tier = tier;
    }
    
    public enum OutputMachineTier
    {
        TIER1 = 2,    // Defines the radius of the machine
        TIER2 = 3,
        TIER3 = 4
    }
    
}