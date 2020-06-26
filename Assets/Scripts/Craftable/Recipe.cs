using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "ScribtableObjects/Recipe")]
public class Recipe : ScriptableObject
{
   public InputItems[] inputs;
   public AbstractMaterial output;
   public int timeToCraft;
}

[Serializable]
public struct InputItems
{
   public AbstractMaterial inputMaterial;
   public int amount;
}