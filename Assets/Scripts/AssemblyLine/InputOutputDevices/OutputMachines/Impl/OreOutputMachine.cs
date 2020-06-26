﻿using System.Collections.Generic;
using UnityEngine;

public class OreOutputMachine : OutputMachine
{
    [SerializeField] private LayerMask _oreLayerMask;
    [SerializeField] private List<MiningTool.MiningSpeed> speedPerMaterial = new List<MiningTool.MiningSpeed>();
    [SerializeField] private GameObject radius;
    
    private List<OreBlock> _inputOres = new List<OreBlock>();
    private Dictionary<OreMaterial, int> _speedPerMaterial;

    

    public void SetOutputMaterial(AbstractMaterial material)
    {
        base.SetOutputMaterial(material);
        // TODO Networking
        //CheckForOre();
    }
    
    public void SetTierMode(OutputMachine.OutputMachineTier tier)
    {
        base.SetTierMode(tier);
        //CheckForOre();
    }
    
    
    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (outputMaterial != null)
        {
            //Mine();
        }
    }

    /*
    public void CheckForOre()
    {
        _inputOres.Clear();
        Inventory.slots[0].Amount = 0;
        Inventory.slots[0] = new InventorySlot(null);
        float scale = _gameSettings.GRIDSIZE * (int) _tier;
        Vector3 pos = transform.position - new Vector3(0, _gameSettings.GRIDSIZE, 0);
        
        Vector3 vertex = new Vector3(scale, 1, scale);
        Collider[] colliders = Physics.OverlapBox(pos, vertex, Quaternion.identity, _oreLayerMask);
        if (radius != null)
        {
            radius.transform.position = pos;
            radius.transform.localScale = vertex;
        }
        foreach (var c in  colliders)
        {
            if (c.gameObject.CompareTag("Ore"))
            {
                Ore ore = c.gameObject.GetComponent<Ore>();
                if (ore.material == outputMaterial)
                {
                    _inputOres.Add(new OreBlock(ore));
                }
            }
        }
        Debug.Log("OutputMachine found: " + _inputOres.Count + " Ores (Tier: " + (int)_tier + ")");
        _speedPerMaterial = new Dictionary<OreMaterial, int>();
        // Map the custom struct to a dictionary
        foreach (var speedItem in speedPerMaterial)
        {
            _speedPerMaterial.Add(speedItem.material, speedItem.unitsPerSecond);
        }
        
    }
    
    private void Mine()
    {
        if(_inputOres.Count <= 0)
            CheckForOre();

        Inventory.slots[0].material = outputMaterial;

        foreach (var block in _inputOres)
        {
            if (_speedPerMaterial.ContainsKey(block.ore.material))
            {
                block.lastMinedDelta += Time.deltaTime * _speedPerMaterial[block.ore.material];
                int wholeUnits = (int)block.lastMinedDelta;
                if(wholeUnits > 0)
                {
                    int delta = wholeUnits + Inventory.slots[Inventory.GetSlotOfMaterial(outputMaterial)].Amount - outputMaterial.maxNumsPerInventorySlot;    // Should not extend maxItemStorage
                    if (delta > 0)
                    {
                        wholeUnits -= delta;
                    }
                    if (wholeUnits > block.ore.units)
                        wholeUnits = block.ore.units;
                    
                    int inventoryOverflow = Inventory.AddItem(outputMaterial, wholeUnits);
                    block.ore.units -= wholeUnits;
                    block.lastMinedDelta -=  wholeUnits;
                }
            }   
        }   
    }
    
    */
    
    public class OreBlock
    {
        public Ore ore;
        public float lastMinedDelta;

        public OreBlock(Ore ore)
        {
            this.ore = ore;
            lastMinedDelta = 0;
        }
    }
}
