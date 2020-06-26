using System;
using System.Collections.Generic;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects.Materials;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using UnityEngine;
using Material = SimpleFactoryServerLib.Objects.Materials.Material;

public abstract class StorageRenderer : MonoBehaviour
{
    public float speed = 50f;

    public GameObject stone;
    public GameObject copper;
    public GameObject coal;
    public GameObject compressedCoal;
    public GameObject diamond;
    public GameObject copperIngots;
    public GameObject copperWire;
    public GameObject cable;
    public GameObject oil;
    public GameObject plastic;

    public NetworkObject networkObject;

    protected WorldObject _worldObject;

    [SerializeField] private GameObject _mostStoredMaterialGameObject;
    [SerializeField] private Material _lastMostStoredMaterial;

    private void Start()
    {
        try
        {
            _worldObject = (ProcessingWorldObject) networkObject.WorldObject;
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e);
        }
    }

    void Update()
    {
        try
        {
            _worldObject = networkObject.WorldObject;
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e);
        }

        if (_worldObject == null)
        {
            deactivateAllPreviews();
            return;
        }

        Material mostStoredMaterial = GetMaterialToRender();

        if (mostStoredMaterial == null)
        {
            // Reset Preview
            deactivateAllPreviews();
            _mostStoredMaterialGameObject = null;
            _lastMostStoredMaterial = null;

            return;
        }

        // No need to set the material preview
        if (mostStoredMaterial.Equals(_lastMostStoredMaterial))
        {
            transform.Rotate(Vector3.up * (speed * Time.deltaTime));
            return;
        }

        // If most stored material changes
        deactivateAllPreviews();

        switch (mostStoredMaterial.name)
        {
            case "Coal":
                _mostStoredMaterialGameObject = coal;
                break;
            case "Copper":
                _mostStoredMaterialGameObject = copper;
                break;
            case "Stone":
                _mostStoredMaterialGameObject = stone;
                break;
            case "Compressed Coal":
                _mostStoredMaterialGameObject = compressedCoal;
                break;
            case "Diamond":
                _mostStoredMaterialGameObject = diamond;
                break;
            case "Copper Ingots":
                _mostStoredMaterialGameObject = copperIngots;
                break;
            case "Copper Wire":
                _mostStoredMaterialGameObject = copperWire;
                break;
            case "Cable":
                _mostStoredMaterialGameObject = cable;
                break;
            case "Oil":
                _mostStoredMaterialGameObject = oil;
                break;
            case "Plastic":
                _mostStoredMaterialGameObject = plastic;
                break;
            default:
                Debug.LogError("Trying to render unknown Material " + mostStoredMaterial.name);
                return;
        }

        _lastMostStoredMaterial = mostStoredMaterial;
        _mostStoredMaterialGameObject.SetActive(true);
        transform.Rotate(Vector3.up * (speed * Time.deltaTime));
    }

    private void deactivateActiveMaterial()
    {
        _mostStoredMaterialGameObject.SetActive(false);
    }

    protected void deactivateAllPreviews()
    {
        stone.SetActive(false);
        coal.SetActive(false);
        copper.SetActive(false);
        compressedCoal.SetActive(false);
        diamond.SetActive(false);
        copperIngots.SetActive(false);
        copperWire.SetActive(false);
        cable.SetActive(false);
        oil.SetActive(false);
        plastic.SetActive(false);
    }

    protected abstract Material GetMaterialToRender();
}