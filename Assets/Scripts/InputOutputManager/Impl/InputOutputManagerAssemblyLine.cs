using System;
using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using UnityEngine;
using UnityEngine.Serialization;

public class InputOutputManagerAssemblyLine : InputOutputManager
{
    public AssemblyLinePrefabMap assemblyLinePrefabMap;
    public AssemblyLineActivator activator;

    public override void updateObject(WorldObject worldObject)
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        SimpleFactoryServerLib.Objects.AssemblyLine assemblyLine =
            (SimpleFactoryServerLib.Objects.AssemblyLine) worldObject;
        activator.updateVisuals(assemblyLine);
    }

    protected override void buildWithInputAndOutput()
    {
        if (Input != Output)
        {
            // --- Update Input Object ---
            updateInputObject();
            // --- End Update Input Object ---

            // --- Update own Object ---
            Vector3 from = transform.position - Input.transform.position;
            Vector3 to = transform.position - Output.transform.position;

            updateOwnObject(from, to);
            // --- End Update own Object ---
        }
        else
        {
            Input = null;
            buildWithOutput();
        }
    }

    protected override void buildWithOutput()
    {
        // --- Update own Object ---
        Vector3 to = Output.transform.position - transform.position;
        Vector3 from = -to;


        updateOwnObject(from, to);
        // --- End Update own Object ---   
    }

    protected override void buildWithInput()
    {
        // --- Update Input Object ---
        updateInputObject();
        // --- End Update Input Object ---

        // --- Update own Object ---
        Vector3 from = transform.position - Input.transform.position;
        Vector3 to = -from;

        updateOwnObject(from, to);
        // --- End Update own Object ---
    }

    protected override void buildWithNothing()
    {
        // --- Update own Object ---
        Vector3 from = Vector3.forward;
        Vector3 to = Vector3.back;

        updateOwnObject(from, to);
        // --- End Update own Object ---
    }

    private void updateOwnObject(Vector3 from, Vector3 to)
    {
        AssemblyLinePrefabMap.PrefabRotation prefabRotation = assemblyLinePrefabMap.getAssemblyLinePrefab(from, to);

        SimpleFactoryServerLib.Objects.AssemblyLine assemblyLine =
            (SimpleFactoryServerLib.Objects.AssemblyLine) GetComponent<NetworkObject>().WorldObject;
        assemblyLine.Prefab = prefabRotation.assemblyLineEnum;
        assemblyLine.rotation = NetworkUtil.convertQuaternionToPosition(prefabRotation.rotation);

        if (Output != null)
        {
            assemblyLine.outputPosition = NetworkUtil.convertVector3ToPosition(Output.transform.position);
        }

        InstantiateMessage instantiateMessage = new InstantiateMessage(assemblyLine);
        networkController.sendToServer(instantiateMessage);

        Debug.Log("Send to server is called " + instantiateMessage);
    }
}