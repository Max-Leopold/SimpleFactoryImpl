
using UnityEngine;

[CreateAssetMenu(fileName = "New Craftable Material", menuName = "ScribtableObjects/Materials/Craftable Material")]
public class CraftableMaterial : AbstractMaterial
{
    // Start is called before the first frame update
    void Start()
    {
        base.Type = MaterialType.CRAFTABLE;
    }

    
}
