using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SimpleFactoryServerLib.Objects.Materials;
using UnityEngine;
using Material = SimpleFactoryServerLib.Objects.Materials.Material;

[CreateAssetMenu(fileName = "New Game Settings", menuName = "ScribtableObjects/Settings")]
public class GameSettings : ScriptableObject
{
    public float GRIDSIZE = 1;
    public float ASSEMBLY_LINE_SIZE = 1;
    public bool uiOverlayVisible = false;

    public Sprite InventoryEmptySlot;
    public float INTERACTION_DISTANCE = 3;    // Used to interact with machine UIs
    
    public List<SimpleFactoryServerLib.Objects.Recipe.Recipe> recipes = new List<SimpleFactoryServerLib.Objects.Recipe.Recipe>();
    public List<AbstractMaterial> oreMaterials = new List<AbstractMaterial>();

    public readonly string TAG_ORE = "Ore";

    // Network Stuff
    public IPEndPoint serverEP;
    public Socket socket;
    public UdpClient udpClient;


    public AbstractMaterial convertMaterialToUnityMaterial(Material mat)
    {
        if (mat == null)
            return null;
        foreach (var m in oreMaterials)
        {
            if (m.title == mat.name)
                return m;
        }
        return null;
    }
    
    public Material convertUnityMaterialToServerMaterial(AbstractMaterial mat)
    {
        return new Material(mat.title);
    }
}
