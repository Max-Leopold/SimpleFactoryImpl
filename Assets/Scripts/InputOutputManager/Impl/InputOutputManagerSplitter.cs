using System.Collections;
using System.Collections.Generic;
using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using UnityEngine;

public class InputOutputManagerSplitter : InputOutputManager
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
        MultiOutputWorldObject splitter =
            (MultiOutputWorldObject) GetComponent<NetworkObject>().WorldObject;
        
        if (actualOutputs.Count >= 1)
        {
            List<Position> positions = new List<Position>();
            foreach (GameObject gameObject in actualOutputs)
            {
                if (gameObject != null)
                {
                    positions.Add(NetworkUtil.convertVector3ToPosition(gameObject.transform.position));   
                }
            }
            splitter.outputPosition = positions;
        }
        
        InstantiateMessage instantiateMessage = new InstantiateMessage(splitter);
        networkController.sendToServer(instantiateMessage);
    }
}
