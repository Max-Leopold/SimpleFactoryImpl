using System;
using UnityEngine;
using Material = SimpleFactoryServerLib.Objects.Materials.Material;

public class OreOutputMachineRenderer : StorageRenderer
{
    protected override Material GetMaterialToRender()
    {
        if (_worldObject is SimpleFactoryServerLib.Objects.OreOutputMachine)
        {
            SimpleFactoryServerLib.Objects.OreOutputMachine castedWObject =
                (SimpleFactoryServerLib.Objects.OreOutputMachine) _worldObject;

            if (castedWObject?.outputMaterial != null)
            {
                return castedWObject.outputMaterial;
            }
        }

        return null;
    }
}