using System.Collections;
using System.Collections.Generic;
using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using UnityEngine;

public class InputOutputManagerProcessingMachine : InputOutputManager
{
     protected override void buildWithInputAndOutput()
    {
        // --- Update Input Object ---
        updateInputObject();
        // --- End Update Input Object ---
        
        // --- Update own Object ---
        updateOwnObject();
        // --- End Update own Object ---
    }

    protected override void buildWithOutput()
    {
        // --- Update own Object ---
        updateOwnObject();
        // --- End Update own Object ---
    }

    protected override void buildWithInput()
    {
        // --- Update Input Object ---
        updateInputObject();
        // --- End Update Input Object ---
        
        // --- Update own Object ---
        updateOwnObject();
        // --- End Update own Object ---

    }
    
    protected override void buildWithNothing()
    {
        // --- Update own Object ---
        updateOwnObject();
        // --- End Update own Object ---
    }

    public override void updateObject(WorldObject worldObject)
    {
        //Don't need to update visuals
    }

    protected override void updateTags()
    {
        //Don't need
    }
    
    private void updateOwnObject()
    {
        SingleOutputWorldObject combiner =
            (SingleOutputWorldObject) GetComponent<NetworkObject>().WorldObject;
        
        if (Output != null)
        {
            combiner.outputPosition = NetworkUtil.convertVector3ToPosition(Output.transform.position);
        }

        combiner.position = NetworkUtil.convertVector3ToPosition(transform.position);

        InstantiateMessage instantiateMessage = new InstantiateMessage(combiner);
        networkController.sendToServer(instantiateMessage);
    }
}
