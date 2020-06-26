using System;
using System.Collections.Generic;
using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Objects.Materials;
using UnityEngine;
using Material = SimpleFactoryServerLib.Objects.Materials.Material;

public class MiningTool : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    public GameSettings gameSettings;
    public GameObject laserBeam;
    public float maxMiningDistance = 20;

    private Material _lastMaterial;
    private float _lastMineDelta;
    private Dictionary<Material, int> _miningSpeedPerSecond = new Dictionary<Material, int>();
    private NetworkController _networkController;


    private void Awake()
    {
        CreateMiningSpeedDictionary();

        _networkController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
        laserBeam.SetActive(false);
    }

    private void CreateMiningSpeedDictionary()
    {
        _miningSpeedPerSecond.Add(Materials.coal, 5);
        _miningSpeedPerSecond.Add(Materials.copper, 10);
        _miningSpeedPerSecond.Add(Materials.stone, 7);
    }

    void Update()
    {
        if (!gameSettings.uiOverlayVisible)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                laserBeam.SetActive(true);
            }

            if (Input.GetKey(KeyCode.Mouse0))
            {
                Mine();
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0) || gameSettings.uiOverlayVisible)
        {
            _lastMaterial = null;
            _lastMineDelta = 0;
            laserBeam.SetActive(false);
        }
    }

    void Mine()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxMiningDistance))
        {
            Debug.DrawLine(Camera.main.transform.position, hit.point);
            Vector3 scale = laserBeam.transform.localScale;
            Vector3 distance = (hit.point - laserBeam.transform.position);
            scale.z = distance.magnitude;
            laserBeam.transform.localScale = scale;
            laserBeam.transform.LookAt(hit.point, transform.forward);

            if (hit.transform.tag == "Ore")
            {
                SimpleFactoryServerLib.Objects.Materials.Ore ore =
                    (SimpleFactoryServerLib.Objects.Materials.Ore) hit.transform.GetComponent<Ore>().WorldObject;
                //Ore ore = hit.transform.GetComponent<Ore>();
                if (!_miningSpeedPerSecond.ContainsKey(ore.material))
                {
                    Debug.LogError("Material's mining speed is not defined in " + this.name);
                    return;
                }

                if (ore.material != _lastMaterial)
                {
                    _lastMineDelta = 0;
                    _lastMaterial = ore.material;
                }

                _lastMineDelta += Time.deltaTime * _miningSpeedPerSecond[ore.material];
                int wholeUnits = (int) _lastMineDelta;
                if (wholeUnits > 0)
                {
                    int toRemove = Math.Min(wholeUnits, ore.Amount);
                    ore.removeSpecificAmount(toRemove);
                    inventory.AddItem(gameSettings.convertMaterialToUnityMaterial(ore.material), toRemove);
                    if (ore.Amount > 0)
                    {
                        //Update ore amount on server
                        InstantiateMessage updateMessage = new InstantiateMessage(ore);
                        _networkController.sendToServer(updateMessage);
                    }
                    else
                    {
                        DeleteMessage deleteMessage = new DeleteMessage(ore);
                        _networkController.sendToServer(deleteMessage);
                    }

                    _lastMineDelta -= wholeUnits;
                }
            }
        }
        else
        {
            _lastMineDelta = 0;
            _lastMaterial = null;
        }
    }

    [Serializable]
    public struct MiningSpeed
    {
        public Material material;
        public int unitsPerSecond;
    }
}