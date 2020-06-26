using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public bool initialScanDone;

    public bool foundOtherInput;
    public bool foundOtherOutput;
    public bool foundOtherUnsure;
    
    public Material unsureMaterial;
    public Material inputMaterial;
    public Material outputMaterial;

    public InputOutputManager inputOutputManager;

    private GameObject _other;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Output"))
        {
            foundOtherOutput = true;
            Other = other.gameObject;
            Debug.Log("Found other output at " + other.transform.position);
        } else if (other.gameObject.CompareTag("Input"))
        {
            foundOtherInput = true;
            Other = other.gameObject;
            Debug.Log("Found other Input at " + other.transform.position);
        } else if (other.gameObject.CompareTag("Unsure"))
        {
            foundOtherUnsure = true;
            Other = other.gameObject;
            Debug.Log("Found other Unsure at " + other.transform.position);
        }
    }

    private void Update()
    {
        if (gameObject.tag == "Unsure")
        {
            transform.GetChild(0).GetComponent<Renderer>().material = unsureMaterial;
        }
        if (gameObject.tag == "Input")
        {
            transform.GetChild(0).GetComponent<Renderer>().material = inputMaterial;
        }
        if (gameObject.tag == "Output")
        {
            transform.GetChild(0).GetComponent<Renderer>().material = outputMaterial;
        }
    }

    public IEnumerator initialScan() {
        //Wait for Physics Update
        yield return new WaitForFixedUpdate();
        initialScanDone = true;
    }

    public void setTag(String tag)
    {
        gameObject.tag = tag;
    }

    public GameObject Other
    {
        get => _other;
        set => _other = value;
    }
}
