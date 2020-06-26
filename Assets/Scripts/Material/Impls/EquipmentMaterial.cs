using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Material", menuName = "ScribtableObjects/Materials/Equipment Material")]
public class EquipmentMaterial : AbstractMaterial
{
    // TODO add props for equiment objects

    private void Awake()
    {
        Type = MaterialType.EQUIPMENT;
    }
}
