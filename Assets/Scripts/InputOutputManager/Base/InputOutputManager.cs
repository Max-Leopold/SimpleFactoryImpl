using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleFactoryServerLib.Network.Messages;
using SimpleFactoryServerLib.Network.Utils;
using SimpleFactoryServerLib.Objects.ProcessingObjects;
using SimpleFactoryServerLib.Server;
using UnityEngine;

public abstract class InputOutputManager : MonoBehaviour, NetworkObjectListener
{
    private enum lastBuildEnum
    {
        NOT_BUILD,
        NOTHING,
        WITH_INPUT,
        WITH_OUTPUT,
        WITH_INPUT_OUTPUT
    }

    public int inputCapacity = 1;
    public int outputCapacity = 1;
    public bool forceScan;

    [SerializeField] private lastBuildEnum lastBuild = lastBuildEnum.NOT_BUILD;

    [SerializeField] private bool initialScanDone;
    [SerializeField] private bool deactivatedChildren = true;

    [SerializeField] private List<GameObject> inputAl = new List<GameObject>();
    [SerializeField] private List<GameObject> outputAl = new List<GameObject>();
    [SerializeField] private List<GameObject> unsureAl = new List<GameObject>();

    public GameObject input;
    public GameObject output;

    public List<GameObject> actualInputs = new List<GameObject>();
    public List<GameObject> actualOutputs = new List<GameObject>();

    public NetworkController networkController;

    private void Awake()
    {
        startCoroutines();
    }

    private void Start()
    {
        GetComponent<NetworkObject>().networkObjectListener.Add(this);
        networkController = GameObject.Find("NetworkController").GetComponent<NetworkController>();
    }

    private void startCoroutines()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CollisionManager childManager = transform.GetChild(i).GetComponent<CollisionManager>();
            StartCoroutine(childManager.initialScan());
        }
    }

    protected void Update()
    {
        if (!initialScanDone)
        {
            if (checkForInitialScan()
                && (Input == null && Output == null || forceScan)
            )
            {
                calculateOutputs();
                calculateInputs();
                calculateUnsure();
                initialScanDone = true;
                forceScan = false;

                if (inputAl.Count == 0 && outputAl.Count == 0)
                {
                    if (unsureAl.Count >= 2)
                    {
                        if (Input == null)
                        {
                            Input = unsureAl[0].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                            unsureAl[0].GetComponent<CollisionManager>().inputOutputManager.Output = gameObject;
                            unsureAl[0].GetComponent<CollisionManager>().Other = gameObject;
                        }

                        if (Output == null && Output != Input)
                        {
                            Output = unsureAl[1].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                            unsureAl[1].GetComponent<CollisionManager>().inputOutputManager.Input = gameObject;
                            unsureAl[1].GetComponent<CollisionManager>().Other = gameObject;
                        }
                    }
                    else if (unsureAl.Count == 1)
                    {
                        if (Input == null)
                        {
                            Input = unsureAl[0].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                            unsureAl[0].GetComponent<CollisionManager>().inputOutputManager.Output = gameObject;
                            unsureAl[0].GetComponent<CollisionManager>().Other = gameObject;
                        }
                    }
                }
                else if (inputAl.Count >= 1 && outputAl.Count == 0)
                {
                    if (Output == null)
                    {
                        Output = inputAl[0].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                        inputAl[0].GetComponent<CollisionManager>().inputOutputManager.Input = gameObject;
                        inputAl[0].GetComponent<CollisionManager>().Other = gameObject;
                    }


                    if (unsureAl.Any())
                    {
                        if (Input == null && Output != Input)
                        {
                            Input = unsureAl[0].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                            unsureAl[0].GetComponent<CollisionManager>().inputOutputManager.Output = gameObject;
                            unsureAl[0].GetComponent<CollisionManager>().Other = gameObject;
                        }
                    }
                }
                else if (inputAl.Count == 0 && outputAl.Count >= 1)
                {
                    if (Input == null)
                    {
                        Input = outputAl[0].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                        outputAl[0].GetComponent<CollisionManager>().inputOutputManager.Output = gameObject;
                        outputAl[0].GetComponent<CollisionManager>().Other = gameObject;
                    }

                    if (unsureAl.Any())
                    {
                        if (Output == null && Output != Input)
                        {
                            Output = unsureAl[0].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                            unsureAl[0].GetComponent<CollisionManager>().inputOutputManager.Input = gameObject;
                            unsureAl[0].GetComponent<CollisionManager>().Other = gameObject;
                        }
                    }
                }
                else if (inputAl.Count >= 1 && outputAl.Count >= 1)
                {
                    if (Input == null)
                    {
                        Input = outputAl[0].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                        outputAl[0].GetComponent<CollisionManager>().inputOutputManager.Output = gameObject;
                        outputAl[0].GetComponent<CollisionManager>().Other = gameObject;
                    }

                    if (Output == null && Output != Input)
                    {
                        Output = inputAl[0].GetComponent<CollisionManager>().inputOutputManager.gameObject;
                        inputAl[0].GetComponent<CollisionManager>().inputOutputManager.Input = gameObject;
                        inputAl[0].GetComponent<CollisionManager>().Other = gameObject;
                    }
                }

                updateTags();
            }
        }

        if (Input == null && Output == null)
        {
            if (lastBuild != lastBuildEnum.NOTHING)
            {
                buildWithNothing();
                lastBuild = lastBuildEnum.NOTHING;
            }
        }
        else if (Input != null && Output == null)
        {
            if (lastBuild != lastBuildEnum.WITH_INPUT)
            {
                buildWithInput();
                lastBuild = lastBuildEnum.WITH_INPUT;
            }
        }
        else if (Input == null && Output != null)
        {
            if (lastBuild != lastBuildEnum.WITH_OUTPUT)
            {
                buildWithOutput();
                lastBuild = lastBuildEnum.WITH_OUTPUT;
            }
        }
        else if (Input != null && Output != null)
        {
            if (lastBuild != lastBuildEnum.WITH_INPUT_OUTPUT)
            {
                buildWithInputAndOutput();
                lastBuild = lastBuildEnum.WITH_INPUT_OUTPUT;
            }
        }
    }

    protected virtual void updateTags()
    {
        if (Input != null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                CollisionManager childManager = transform.GetChild(i).GetComponent<CollisionManager>();
                if (childManager.Other != Input)
                {
                    childManager.setTag("Output");
                }
                else
                {
                    childManager.setTag("Input");
                }
            }
        }
        else if (Output != null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                CollisionManager childManager = transform.GetChild(i).GetComponent<CollisionManager>();
                if (childManager.Other != Output)
                {
                    childManager.setTag("Input");
                }
                else
                {
                    childManager.setTag("Output");
                }
            }
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                CollisionManager childManager = transform.GetChild(i).GetComponent<CollisionManager>();
                childManager.setTag("Unsure");
            }
        }
    }


    private bool checkForInitialScan()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CollisionManager childManager = transform.GetChild(i).GetComponent<CollisionManager>();
            if (!childManager.initialScanDone)
            {
                return false;
            }
        }

        return true;
    }

    private void calculateOutputs()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CollisionManager childManager = transform.GetChild(i).GetComponent<CollisionManager>();
            if (childManager.foundOtherOutput)
            {
                outputAl.Add(childManager.Other);
            }
        }
    }

    private void calculateInputs()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CollisionManager childManager = transform.GetChild(i).GetComponent<CollisionManager>();
            if (childManager.foundOtherInput)
            {
                inputAl.Add(childManager.Other);
            }
        }
    }

    private void calculateUnsure()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CollisionManager childManager = transform.GetChild(i).GetComponent<CollisionManager>();
            if (childManager.foundOtherUnsure)
            {
                unsureAl.Add(childManager.Other);
            }
        }
    }

    public void setOutput(GameObject gameObject)
    {
        output = gameObject.GetComponent<BuildableObject>().inputOutputManager.gameObject;
        if (!actualOutputs.Contains(gameObject.GetComponent<BuildableObject>().inputOutputManager.gameObject))
        {
            actualOutputs.Add(gameObject.GetComponent<BuildableObject>().inputOutputManager.gameObject);
        }

        gameObject.GetComponent<BuildableObject>().inputOutputManager.Input = this.gameObject;
        gameObject.GetComponent<BuildableObject>().inputOutputManager.updateTags();

        updateTags();
    }

    public void setInput(GameObject gameObject)
    {
        input = gameObject.GetComponent<BuildableObject>().inputOutputManager.gameObject;
        if (!actualInputs.Contains(gameObject.GetComponent<BuildableObject>().inputOutputManager.gameObject))
        {
            actualInputs.Add(gameObject.GetComponent<BuildableObject>().inputOutputManager.gameObject);
        }

        gameObject.GetComponent<BuildableObject>().inputOutputManager.Output = this.gameObject;
        gameObject.GetComponent<BuildableObject>().inputOutputManager.updateTags();

        updateTags();
    }

    public GameObject Input
    {
        get => input;
        set
        {
            input = value;

            if (!actualInputs.Contains(value))
            {
                actualInputs.Add(value);
            }

            updateTags();
        }
    }

    public GameObject Output
    {
        get => output;
        set
        {
            output = value;

            if (!actualOutputs.Contains(value))
            {
                actualOutputs.Add(value);
            }

            updateTags();
        }
    }

    public void deactivateChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void activateChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    protected void updateInputObject()
    {
        if (Input.GetComponent<NetworkObject>().WorldObject is SingleOutputWorldObject)
        {
            SingleOutputWorldObject inputObject =
                (SingleOutputWorldObject) Input.GetComponent<NetworkObject>().WorldObject;
            inputObject.outputPosition = NetworkUtil.convertVector3ToPosition(transform.position);
            InstantiateMessage instantiateMessage = new InstantiateMessage(inputObject);
            networkController.sendToServer(instantiateMessage);
        }
        else if (Input.GetComponent<NetworkObject>().WorldObject is MultiOutputWorldObject)
        {
            MultiOutputWorldObject inputObject =
                (MultiOutputWorldObject) Input.GetComponent<NetworkObject>().WorldObject;
            if (!inputObject.outputPosition.Contains(NetworkUtil.convertVector3ToPosition(transform.position)))
            {
                inputObject.outputPosition.Add(NetworkUtil.convertVector3ToPosition(transform.position));
            }

            InstantiateMessage instantiateMessage = new InstantiateMessage(inputObject);
            networkController.sendToServer(instantiateMessage);
        }
    }

    protected abstract void buildWithInputAndOutput();

    protected abstract void buildWithOutput();

    protected abstract void buildWithInput();

    protected abstract void buildWithNothing();
    public abstract void updateObject(WorldObject worldObject);
}