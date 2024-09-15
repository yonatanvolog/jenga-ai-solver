using UnityEngine;
using TMPro;
using System;
using UnityEditor.Build.Content;
using UnityEngine.UI;

public enum PlayerType
{
    RANDOM = 0,
    DQN = 1,
    SARSA = 2,
    GSBAS = 3,
    HUMAN = 4
}

public class MainMenuManager : MonoBehaviour
{
    public TMP_Dropdown player1Dropdown;
    public TMP_Dropdown player2Dropdown;
    public TMP_Dropdown roundsDropdown; // Add dropdown for number of rounds
    public Button startButton;
    public Button quitButton;
    public GameObject mainMenu; // Reference to the main menu

    private PlayerType player1Selection;
    private PlayerType player2Selection;
    private int num_of_rounds; // To store selected number of rounds
    public CameraController cameraController;
    public bool menu_status = false;
    public CommandDispatcher commandDispatcher;  // Add CommandDispatcher field
    public GameObject gameInfo;
    
    void Start()
    {
        // Find CommandDispatcher in the scene
        if (commandDispatcher == null)
        {
            commandDispatcher = GameObject.FindObjectOfType<CommandDispatcher>();
        }
        
        // Find cameraController in the scene
        if (cameraController == null)
        {
            cameraController = GameObject.FindObjectOfType<CameraController>();
        }

        // Set default value to "Human Player"
        SetDefaultDropdownValue(player1Dropdown, "Human Player");
        SetDefaultDropdownValue(player2Dropdown, "Human Player");

        // Set default rounds value (optional, depending on your dropdown)
        SetDefaultDropdownValue(roundsDropdown, "1"); // Example of default

        // Add listeners for the buttons
        startButton.onClick.AddListener(OnStartButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);

        // Add listeners for TMP dropdowns
        player1Dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(player1Dropdown, 1); });
        player2Dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(player2Dropdown, 2); });
        roundsDropdown.onValueChanged.AddListener(delegate { RoundsDropdownValueChanged(roundsDropdown); });

        // Set default selections to HUMAN
        player1Selection = PlayerType.HUMAN;
        player2Selection = PlayerType.HUMAN;
        num_of_rounds = 1;
    }

    void Update()
    {
        // Enable the main menu if ESC is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            commandDispatcher.DispatchEndGame();
            ToggleMainMenu();
        }

        //Sometimes python misses 
        if ((mainMenu.activeSelf == false && menu_status == true) || 
            (mainMenu.activeSelf == true && menu_status == false))
        {
            ToggleMainMenu();
        }
    }

    void DropdownValueChanged(TMP_Dropdown dropdown, int player)
    {
        PlayerType selectedType = GetPlayerTypeFromDropdown(dropdown.options[dropdown.value].text);

        if (player == 1)
        {
            player1Selection = selectedType;
        }
        else if (player == 2)
        {
            player2Selection = selectedType;
        }
    }

    void RoundsDropdownValueChanged(TMP_Dropdown dropdown)
    {
        string selectedRounds = dropdown.options[dropdown.value].text;
        num_of_rounds = Int32.Parse(selectedRounds.Split(' ')[0]);
    }

    PlayerType GetPlayerTypeFromDropdown(string option)
    {
        switch (option)
        {
            case string s when s.StartsWith("Random"):
                return PlayerType.RANDOM;

            case string s when s.StartsWith("Deep"):
                return PlayerType.DQN;

            case string s when s.StartsWith("SARSA"):
                return PlayerType.SARSA;

            case string s when s.StartsWith("GSBAS"):
                return PlayerType.GSBAS;

            case string s when s.StartsWith("Human Player"):
                return PlayerType.HUMAN;

            default:
                throw new ArgumentException("Unknown player type: " + option);
        }
    }


    void SetDefaultDropdownValue(TMP_Dropdown dropdown, string defaultOption)
    {
        int defaultIndex = dropdown.options.FindIndex(option => option.text == defaultOption);
        if (defaultIndex != -1)
        {
            dropdown.value = defaultIndex;
            dropdown.RefreshShownValue(); // Refreshes the dropdown to show the correct label
        }
    }

    void OnStartButtonPressed()
    {
        Debug.Log("Player 1: " + player1Selection);
        Debug.Log("Player 2: " + player2Selection);
        Debug.Log("Number of Rounds: " + num_of_rounds);

        // Disable the main menu
        ToggleMainMenu();

        // Send "StartGame" command to Python via CommandDispatcher
        StartGame(player1Selection, player2Selection, num_of_rounds);
    }

    void OnQuitButtonPressed()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    void StartGame(PlayerType player1, PlayerType player2, int num_of_rounds)
    {
        // Logic to start the game with the chosen players and number of rounds
        Debug.Log("Starting game with Player 1: " + player1 + " and Player 2: " + player2 + " and Rounds: " + num_of_rounds);
        commandDispatcher.DispatchStartGame((int)player1, (int)player2, num_of_rounds);
        // SceneManager.LoadScene("GameScene"); // Example of loading the game scene
    }

    // Method to toggle the main menu visibility
    public void ToggleMainMenu()
    {
        if (mainMenu.activeSelf == true)
        {
            menu_status = false;
            mainMenu.SetActive(false);
            gameInfo.SetActive(true);
            cameraController.ToggleMenuMode(false);
            cameraController.ToggleMouseControl(true);
        }
        else
        {
            menu_status = true;
            mainMenu.SetActive(true);
            gameInfo.SetActive(false);
            cameraController.ToggleMenuMode(true);
            cameraController.ToggleMouseControl(false);
        }
    }
}
