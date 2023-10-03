using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    // bool to check if game is paused
    public static bool paused = false;

    public static bool onSetPath = false;

    // menu screen
    [SerializeField]
    private GameObject menuScreen;
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject IsoSurfMenu;
    [SerializeField]
    private GameObject StreamLinesMenu;
    [SerializeField]
    private GameObject VisualizationMenu;
    [SerializeField]
    private GameObject TeleportMenu;
    [SerializeField]
    private GameObject OptionsMenu;
    [SerializeField]
    private GameObject MeshtalMenu;
    [SerializeField]
    private GameObject SetMeshtalMenu;
    [SerializeField]
    private GameObject LoadMeshMenu;
    [SerializeField]
    private GameObject rotationMenu;
    [SerializeField]
    private GameObject traslationMenu;
    [SerializeField]
    private GameObject sliceMenu;
    [SerializeField]
    private GameObject maintenanceMenu;
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private GameObject miniMap;
    [SerializeField]
    private GameObject makePath;

    //[SerializeField]
    //private GameObject shortestPathMenu;

    public GameObject setPathMenu;
    public GameObject goPath;

    goTosetPath goTosetPath;

    // action
    PauseAction action;

    public bool wasOptionsEnabled = false;


    // Menu settings
    public enum MenuStates { Main, IsoSurf, StreamLines, Visualize, Teleport, Options, meshtal, load, slice, maintenance, Set, shortestPath };
    public MenuStates currentState;
    MenuStates previousMenuStates;


    // public PlayerInput input;

    public bool isPaused()
    {
        return paused;
    }

    public bool isOnPath()
    {
        return onSetPath;
    }

    // initializa pause action and the main menu setting
    private void Awake()
    {
        panel.SetActive(false);
        menuScreen.SetActive(false);
        mainMenu.SetActive(false);
        IsoSurfMenu.SetActive(false);
        StreamLinesMenu.SetActive(false);
        VisualizationMenu.SetActive(false);
        TeleportMenu.SetActive(false);
        SetMeshtalMenu.SetActive(false);
        OptionsMenu.SetActive(false);
        MeshtalMenu.SetActive(false);
        LoadMeshMenu.SetActive(false);
        sliceMenu.SetActive(false);
        maintenanceMenu.SetActive(false);
        //shortestPathMenu.SetActive(false);
        action = new PauseAction();
        currentState = MenuStates.Main;
    }
    private void OnEnable()
    {
        action.Enable();
    }
    private void OnDisable()
    {
        action.Disable();
    }

    // Determine if a pause is detected
    private void Start()
    {
        action.Pause.PauseGame.performed += _ => DeterminePause();
        goTosetPath = goPath.GetComponent<goTosetPath>();
    }

    private void Update()
    {
        if (paused & !onSetPath)
        {
            switch (currentState)
            {
                case MenuStates.Main:
                    // Set active gameobject
                    mainMenu.SetActive(true);
                    IsoSurfMenu.SetActive(false);
                    StreamLinesMenu.SetActive(false);
                    VisualizationMenu.SetActive(false);
                    TeleportMenu.SetActive(false);
                    OptionsMenu.SetActive(false);
                    SetMeshtalMenu.SetActive(false);
                    sliceMenu.SetActive(false);
                    MeshtalMenu.SetActive(false);
                    LoadMeshMenu.SetActive(false);
                    maintenanceMenu.SetActive(false);
                    //shortestPathMenu.SetActive(false);
                    break;

                case MenuStates.IsoSurf:
                    // Set active gameobject
                    mainMenu.SetActive(false);
                    IsoSurfMenu.SetActive(true);
                    panel.SetActive(true);
                    OptionsMenu.SetActive(false);
                    break;

                case MenuStates.StreamLines:
                    mainMenu.SetActive(false);
                    StreamLinesMenu.SetActive(true);
                    OptionsMenu.SetActive(false);
                    break;
                case MenuStates.Visualize:
                    mainMenu.SetActive(false);
                    VisualizationMenu.SetActive(true);
                    break;
                case MenuStates.Teleport:
                    mainMenu.SetActive(false);
                    TeleportMenu.SetActive(true);
                    break;

                case MenuStates.meshtal:
                    mainMenu.SetActive(false);
                    MeshtalMenu.SetActive(true);
                    LoadMeshMenu.SetActive(false);
                    SetMeshtalMenu.SetActive(false);
                    break;
                case MenuStates.Options:
                    OptionsMenu.SetActive(true);
                    wasOptionsEnabled = true;
                    //mainMenu.SetActive(false);
                    //IsoSurfMenu.SetActive(false);
                    //StreamLinesMenu.SetActive(false);
                    //VisualizationMenu.SetActive(false);
                    //TeleportMenu.SetActive(false);
                    //LoadMeshMenu.SetActive(false);
                    break;
                case MenuStates.load:
                    LoadMeshMenu.SetActive(true);
                    MeshtalMenu.SetActive(false);
                    mainMenu.SetActive(false);
                    break;

                case MenuStates.shortestPath:
                    //shortestPathMenu.SetActive(true);
                    maintenanceMenu.SetActive(false);
                    mainMenu.SetActive(false);
                    break;

                case MenuStates.Set:
                    SetMeshtalMenu.SetActive(true);
                    MeshtalMenu.SetActive(false);
                    mainMenu.SetActive(false);
                    break;
                case MenuStates.maintenance:
                    maintenanceMenu.SetActive(true);
                    mainMenu.SetActive(false);
                    break;
                case MenuStates.slice:
                    sliceMenu.SetActive(true);
                    mainMenu.SetActive(false);
                    break;
            }

        }

    }

    public bool optionsEnabled()
    {
        return wasOptionsEnabled;
    }

    public void OnMainMenu()
    {
        currentState = MenuStates.Main;
    }
    public void OnIsoSurfMenu()
    {
        currentState = MenuStates.IsoSurf;
    }
    public void OnStreamLinesMenu()
    {
        currentState = MenuStates.StreamLines;
    }

    public void OnVisualizationMenu()
    {
        currentState = MenuStates.Visualize;
    }

    public void OnTeleportMenu()
    {
        currentState = MenuStates.Teleport;
    }

    public void OnSliceMenu()
    {
        currentState = MenuStates.slice;
    }

    public void OnMaintenanceMenu()
    {
        currentState = MenuStates.maintenance;
    }

    public void OnOptionsMenu()
    {
        previousMenuStates = currentState;
        currentState = MenuStates.Options;
    }

    public void OnLoadMenu()
    {
        previousMenuStates = currentState;
        currentState = MenuStates.load;
    }

    public void OnSetMenu()
    {
        previousMenuStates = currentState;
        currentState = MenuStates.Set;
    }

    public void OnShortestPathMenu()
    {
        previousMenuStates = currentState;
        currentState = MenuStates.shortestPath;
    }

    public void OnMeshtalMenu()
    {
        currentState = MenuStates.meshtal;
    }

    public void rotationMatrix()
    {
        if (rotationMenu.activeInHierarchy)
        {
            rotationMenu.SetActive(false);
        }
        else
        {
            rotationMenu.SetActive(true);
        }

    }

    public void traslation()
    {
        if (traslationMenu.activeInHierarchy)
        {
            traslationMenu.SetActive(false);
        }
        else
        {
            traslationMenu.SetActive(true);
        }

    }

    public void OnExitOptions()
    {
        currentState = previousMenuStates;
    }

    private void DeterminePause()
    {
        if (paused)
        {
            ResumeGame();
            if (onSetPath)
            {
                setPathOff();
            }
        }
        else
        {
            PauseGame();
            if(onSetPath)
            {
                setPathOff();
            }
        }
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        paused = true;
        menuScreen.SetActive(true);
    }
    public void ResumeGame()
    {
        //wasOptionsEnabled = false;
        Time.timeScale = 1;
        paused = false;
        menuScreen.SetActive(false);
        mainMenu.SetActive(false);
        IsoSurfMenu.SetActive(false);
        StreamLinesMenu.SetActive(false);
        VisualizationMenu.SetActive(false);
        TeleportMenu.SetActive(false);
        OptionsMenu.SetActive(false);
        MeshtalMenu.SetActive(false);
        LoadMeshMenu.SetActive(false);
        sliceMenu.SetActive(false);
        maintenanceMenu.SetActive(false);
        traslationMenu.SetActive(false);
        rotationMenu.SetActive(false);
        //shortestPathMenu.SetActive(false);
        currentState = MenuStates.Main;
    }

    public void setPathOn()
    {
        goPath.SetActive(true);
        miniMap.SetActive(false);
        Time.timeScale = 1;
        onSetPath = true;
        menuScreen.SetActive(false);
        //shortestPathMenu.SetActive(false);
        maintenanceMenu.SetActive(false);
        setPathMenu.SetActive(true);

        goTosetPath.changeCamera();
        
    }

    public void setPathOff()
    {
        Time.timeScale = 0;
        miniMap.SetActive(true);
        goTosetPath.exitMode();
        paused = true;
        onSetPath = false;
        menuScreen.SetActive(true);
        //shortestPathMenu.SetActive(true);
        maintenanceMenu.SetActive(true);
        setPathMenu.SetActive(false);
        goPath.SetActive(false);
        makePath.SetActive(false);
    }

}

