using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ore Material", menuName = "ScribtableObjects/Materials/Ore Material", order = 2)]
public class OreMaterial : AbstractMaterial
{
    /// <summary>
    /// The weight of one of this ore
    /// </summary>
    public float weight;

    private void Awake()
    {
        Type = MaterialType.ORE;
    }
}
