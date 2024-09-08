using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public enum PlayerType
{
    RANDOM,
    DQN,
    SARSA,
    GSBAS,
    HUMAN
}

public class MainMenuManager : MonoBehaviour
{
    public TMP_Dropdown player1Dropdown;
    public TMP_Dropdown player2Dropdown;
    public Button startButton;
    public Button quitButton;
    public GameObject mainMenu; // Reference to the main menu

    private PlayerType player1Selection;
    private PlayerType player2Selection;

    public CommandDispatcher commandDispatcher;  // Add CommandDispatcher field

    void Start()
    {
        // Find CommandDispatcher in the scene
        if (commandDispatcher == null)
        {
            commandDispatcher = GameObject.FindObjectOfType<CommandDispatcher>();
        }

        // Set default value to "Human Player"
        SetDefaultDropdownValue(player1Dropdown, "Human Player");
        SetDefaultDropdownValue(player2Dropdown, "Human Player");

        // Add listeners for the buttons
        startButton.onClick.AddListener(OnStartButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);

        // Add listeners for TMP dropdowns
        player1Dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(player1Dropdown, 1); });
        player2Dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(player2Dropdown, 2); });

        // Set default selections to HUMAN
        player1Selection = PlayerType.HUMAN;
        player2Selection = PlayerType.HUMAN;
    }

    void Update()
    {
        // Enable the main menu if ESC is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMainMenu(true);
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

    PlayerType GetPlayerTypeFromDropdown(string option)
    {
        switch (option)
        {
            case "Human Player":
                return PlayerType.HUMAN;
            case "AI - Deep Q Network":
                return PlayerType.DQN;
            case "AI - Monte Carlo Tree Search":
                return PlayerType.SARSA;
            case "AI - Random":
                return PlayerType.RANDOM;
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

        // Disable the main menu
        ToggleMainMenu(false);

        // Send "StartGame" command to Python via CommandDispatcher
        commandDispatcher.SendCommand(CommandDispatcher.Command.StartGame);

        // Save the selections and start the game
        StartGame(player1Selection, player2Selection);
    }

    void OnQuitButtonPressed()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    void StartGame(PlayerType player1, PlayerType player2)
    {
        // Logic to start the game with the chosen players
        Debug.Log("Starting game with Player 1: " + player1 + " and Player 2: " + player2);
        //commandDispatcher.SendCommand(CommandDispatcher.Command.StartGame);
        commandDispatcher.DispatchStartGame((int)player1, (int)player2);
        //SceneManager.LoadScene("GameScene"); // Example of loading the game scene
    }

    void ToggleMainMenu(bool isEnabled)
    {
        mainMenu.SetActive(isEnabled); // Enable or disable the main menu
    }
}
