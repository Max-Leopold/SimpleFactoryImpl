using System;
using System.Collections;
using System.Collections.Generic;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using UnityEngine;
using Material = SimpleFactoryServerLib.Objects.Materials.Material;

public class NetworkObject : MonoBehaviour
{

    public List<NetworkObjectListener> networkObjectListener = new List<NetworkObjectListener>();
    
    [SerializeField]
    private WorldObject _worldObject;

    public List<Material> materialStorage;
    public List<int> storageSize;

    private void Update()
    {
        if (_worldObject != null && _worldObject is ProcessingWorldObject)
        {
            ProcessingWorldObject pwo = (ProcessingWorldObject) _worldObject;
            materialStorage = new List<Material>(pwo.storage.Keys);
            storageSize = new List<int>(pwo.storage.Values);
        }
    }

    public WorldObject WorldObject
    {
        get => _worldObject;
        set
        {
            _worldObject = value;
            transform.rotation = Quaternion.Euler(new Vector3(value.rotation.x, value.rotation.y, value.rotation.z));
            
            Debug.Log("Call updateObject on " + value);
            foreach (NetworkObjectListener listener in networkObjectListener)
            {
                listener?.updateObject(value);
            }
        }
    }
}
