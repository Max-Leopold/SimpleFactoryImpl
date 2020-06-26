using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    public InputOutputManager inputOutputManager;

    private void Start()
    {
        if (inputOutputManager.inputCapacity > inputOutputManager.actualInputs.Count ||
            inputOutputManager.outputCapacity > inputOutputManager.actualOutputs.Count)
        {
            inputOutputManager.activateChildren();
        }
        else
        {
            inputOutputManager.deactivateChildren();
        }
    }

    private void Update()
    {
        if (inputOutputManager.inputCapacity == inputOutputManager.actualInputs.Count &&
            inputOutputManager.outputCapacity == inputOutputManager.actualOutputs.Count)
        {
            inputOutputManager.deactivateChildren();
        }
        else
        {
            inputOutputManager.activateChildren();
        }
    }
}