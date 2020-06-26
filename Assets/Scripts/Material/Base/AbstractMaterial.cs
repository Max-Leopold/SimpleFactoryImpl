using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MaterialType
{
    ORE,
    EQUIPMENT,
    CRAFTABLE
}

public abstract class AbstractMaterial : ScriptableObject
{
    public GameObject prefab;
    public string title;
    [TextArea(10, 18)]
    public string description;
    public Sprite uiSprite;
    public int maxNumsPerInventorySlot;
    public MaterialType Type
    {
        get;
        protected set;
    }
}
