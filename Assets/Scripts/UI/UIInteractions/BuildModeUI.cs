using UnityEngine;

public class BuildModeUI : MonoBehaviour
{
    private GameObject buildModeSelect;

    public GameObject buildModeAssemblyLine;
    public GameObject buildModeAssemblyLineStar;
    public GameObject buildModeOreMachine;
    public GameObject buildModeSplitter;
    public GameObject buildModeCombiner;
    public GameObject buildModeMachine;
    public GameObject buildModeChest;
    public GameObject buildModeDelete;

    private void Start()
    {
        buildModeSelect = buildModeAssemblyLine;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            buildModeSelect.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            buildModeSelect.SetActive(false);
            buildModeAssemblyLine.SetActive(true);
            buildModeSelect = buildModeAssemblyLine;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            buildModeSelect.SetActive(false);
            buildModeAssemblyLineStar.SetActive(true);
            buildModeSelect = buildModeAssemblyLineStar;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            buildModeSelect.SetActive(false);
            buildModeOreMachine.SetActive(true);
            buildModeSelect = buildModeOreMachine;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            buildModeSelect.SetActive(false);
            buildModeSplitter.SetActive(true);
            buildModeSelect = buildModeSplitter;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            buildModeSelect.SetActive(false);
            buildModeCombiner.SetActive(true);
            buildModeSelect = buildModeCombiner;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            buildModeSelect.SetActive(false);
            buildModeMachine.SetActive(true);
            buildModeSelect = buildModeMachine;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            buildModeSelect.SetActive(false);
            buildModeChest.SetActive(true);
            buildModeSelect = buildModeChest;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            buildModeSelect.SetActive(false);
            buildModeDelete.SetActive(true);
            buildModeSelect = buildModeDelete;
        }
    }
}