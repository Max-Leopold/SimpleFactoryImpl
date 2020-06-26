using System.Collections;
using System.Collections.Generic;
using SimpleFactoryServerLib.Objects;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using UnityEngine;
using Material = SimpleFactoryServerLib.Objects.Materials.Material;

public class AssemblyLineStorageRenderer : StorageRenderer
{
    protected override Material GetMaterialToRender()
    {
        Material mostStoredMaterial = null;
        int storageCount = 0;
        
        if (_worldObject is ProcessingWorldObject)
        {
            ProcessingWorldObject castedWObject = (ProcessingWorldObject) _worldObject;
            if (castedWObject?.storage != null && castedWObject.storage.Count > 0)
            {
                foreach (KeyValuePair<Material,int> pair in castedWObject.storage)
                {
                    if (pair.Value > storageCount)
                    {
                        storageCount = pair.Value;
                        mostStoredMaterial = pair.Key;
                    }
                }
            }
        }

        return mostStoredMaterial;
    }
}
