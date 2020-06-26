using System.Collections;
using System.Collections.Generic;
using SimpleFactoryServerLib.Objects;
using UnityEngine;

public class AssemblyLineActivator : MonoBehaviour
{
    public GameObject AssemblyLine_X_Z_Straight;
    public GameObject AssemblyLine_X_Z_Curve;
    public GameObject AssemblyLine_X_Y_Straight;
    public GameObject AssemblyLine_Y_Y;
    public GameObject AssemblyLine_Y_X_Straight;

    public void updateVisuals(SimpleFactoryServerLib.Objects.AssemblyLine assemblyLine)
    {
        deactivateAll();  
        
        if (assemblyLine.Prefab == AssemblyLineEnum.EmptyAssemblyLine)
        {
            Debug.Log("Activate Visual: EmptyAssemblyLine");
        }
        else if (assemblyLine.Prefab == AssemblyLineEnum.AssemblyLine_Y_Y)
        {
            Debug.Log("Activate Visual: AssemblyLine_Y_Y");
            AssemblyLine_Y_Y.SetActive(true);
            AssemblyLine_Y_Y.transform.rotation = Quaternion.Euler(new Vector3(assemblyLine.rotation.x, assemblyLine.rotation.y, assemblyLine.rotation.z));
           // GetComponent<AssemblyLine>().objAtThisPos = AssemblyLine_Y_Y;
        }
        else if (assemblyLine.Prefab == AssemblyLineEnum.AssemblyLine_X_Y_Straight)
        {
            Debug.Log("Activate Visual: AssemblyLine_X_Y_Straight");
            AssemblyLine_X_Y_Straight.SetActive(true);
            AssemblyLine_X_Y_Straight.transform.rotation = Quaternion.Euler(new Vector3(assemblyLine.rotation.x, assemblyLine.rotation.y, assemblyLine.rotation.z));
            //GetComponent<AssemblyLine>().objAtThisPos = AssemblyLine_X_Y_Straight;
        }
        else if (assemblyLine.Prefab == AssemblyLineEnum.AssemblyLine_X_Z_Curve)
        {
            Debug.Log("Activate Visual: AssemblyLine_X_Z_Curve");
            AssemblyLine_X_Z_Curve.SetActive(true);
            AssemblyLine_X_Z_Curve.transform.rotation = Quaternion.Euler(new Vector3(assemblyLine.rotation.x, assemblyLine.rotation.y, assemblyLine.rotation.z));
            //GetComponent<AssemblyLine>().objAtThisPos = AssemblyLine_X_Z_Curve;
        }
        else if (assemblyLine.Prefab == AssemblyLineEnum.AssemblyLine_X_Z_Straight)
        {
            Debug.Log("Activate Visual: AssemblyLine_X_Z_Straight");
            AssemblyLine_X_Z_Straight.SetActive(true);
            AssemblyLine_X_Z_Straight.transform.rotation = Quaternion.Euler(new Vector3(assemblyLine.rotation.x, assemblyLine.rotation.y, assemblyLine.rotation.z));
            //GetComponent<AssemblyLine>().objAtThisPos = AssemblyLine_X_Z_Straight;
        }
        else
        {
            Debug.Log("Activate Visual: AssemblyLine_Y_X_Straight");
            AssemblyLine_Y_X_Straight.SetActive(true);
            AssemblyLine_Y_X_Straight.transform.rotation = Quaternion.Euler(new Vector3(assemblyLine.rotation.x, assemblyLine.rotation.y, assemblyLine.rotation.z));
            //GetComponent<AssemblyLine>().objAtThisPos = AssemblyLine_Y_X_Straight;
        }
    }

    private void deactivateAll()
    {
        AssemblyLine_X_Z_Straight.SetActive(false);
        AssemblyLine_X_Z_Curve.SetActive(false);
        AssemblyLine_X_Y_Straight.SetActive(false);
        AssemblyLine_Y_Y.SetActive(false);
        AssemblyLine_Y_X_Straight.SetActive(false);
    }
}