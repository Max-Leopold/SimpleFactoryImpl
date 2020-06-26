using SimpleFactoryServerLib.Network.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Material = UnityEngine.Material;

public class Ore : NetworkObject, NetworkObjectListener
{
    public Material copper, coal, stone;

    [FormerlySerializedAs("_amount")] [SerializeField]
    private int amount;
    private void Start()
    {
        networkObjectListener.Add(this);
    }

    public void updateColor()
    {
        SimpleFactoryServerLib.Objects.Materials.Ore ore = (SimpleFactoryServerLib.Objects.Materials.Ore) WorldObject;
        if (ore.material.name == "Coal")
        {
            GetComponent<MeshRenderer>().material = coal;
        }else if (ore.material.name == "Copper")
        {
            GetComponent<MeshRenderer>().material = copper;
        }else if (ore.material.name == "Stone")
        {
            GetComponent<MeshRenderer>().material = stone;
        }
    }

    public void updateObject(WorldObject worldObject)
    {
        //Debug.LogError("Hello");
        amount = ((SimpleFactoryServerLib.Objects.Materials.Ore) worldObject).Amount;
        // Maybe display available amount of ore here
    }
}
