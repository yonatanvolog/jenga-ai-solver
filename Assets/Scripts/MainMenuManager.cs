using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public enum PlayerType
{
    RANDOM,
    DQN,
    SARSA,
    HUMAN
}

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown player1Dropdown;
    [SerializeField] private TMP_Dropdown player2Dropdown;
    [SerializeField] private TMP_Dropdown roundsDropdown;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CommandDispatcher commandDispatcher;
    [SerializeField] private GameObject gameInfo;

    private PlayerType player1Selection;
    private PlayerType player2Selection;
    private int num_of_rounds;
    private bool menu_status = false;

    void Start()
    {
        if (commandDispatcher == null)
        {
            commandDispatcher = FindObjectOfType<CommandDispatcher>();
        }

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }

        SetDefaultDropdownValue(player1Dropdown, "Human Player");
        SetDefaultDropdownValue(player2Dropdown, "Human Player");
        SetDefaultDropdownValue(roundsDropdown, "1");

        startButton.onClick.AddListener(OnStartButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);

        player1Dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(player1Dropdown, 1); });
        player2Dropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(player2Dropdown, 2); });
        roundsDropdown.onValueChanged.AddListener(delegate { RoundsDropdownValueChanged(roundsDropdown); });

        player1Selection = PlayerType.HUMAN;
        player2Selection = PlayerType.HUMAN;
        num_of_rounds = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !mainMenu.activeSelf)
        {
            commandDispatcher.DispatchEndGame();
            ToggleMainMenu();
        }

        //Sometimes python misses Toggle Menu command
        if ((!mainMenu.activeSelf && menu_status) || (mainMenu.activeSelf && !menu_status))
        {
            ToggleMainMenu();
        }
    }

    private void DropdownValueChanged(TMP_Dropdown dropdown, int player)
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

    private void RoundsDropdownValueChanged(TMP_Dropdown dropdown)
    {
        string selectedRounds = dropdown.options[dropdown.value].text;
        num_of_rounds = Int32.Parse(selectedRounds.Split(' ')[0]);
    }

    private PlayerType GetPlayerTypeFromDropdown(string option)
    {
        return option switch
        {
            string s when s.StartsWith("Random") => PlayerType.RANDOM,
            string s when s.StartsWith("Deep") => PlayerType.DQN,
            string s when s.StartsWith("SARSA") => PlayerType.SARSA,
            string s when s.StartsWith("Human Player") => PlayerType.HUMAN,
            _ => throw new ArgumentException("Unknown player type: " + option),
        };
    }

    private void SetDefaultDropdownValue(TMP_Dropdown dropdown, string defaultOption)
    {
        int defaultIndex = dropdown.options.FindIndex(option => option.text == defaultOption);
        if (defaultIndex != -1)
        {
            dropdown.value = defaultIndex;
            dropdown.RefreshShownValue();
        }
    }

    private void OnStartButtonPressed()
    {
        Debug.Log($"Player 1: {player1Selection}, Player 2: {player2Selection}, Number of Rounds: {num_of_rounds}");

        ToggleMainMenu();
        StartGame(player1Selection, player2Selection, num_of_rounds);
    }

    private void OnQuitButtonPressed()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private void StartGame(PlayerType player1, PlayerType player2, int num_of_rounds)
    {
        Debug.Log($"Starting game with Player 1: {player1}, Player 2: {player2}, Rounds: {num_of_rounds}");
        commandDispatcher.DispatchStartGame((int)player1, (int)player2, num_of_rounds);
    }

    public void ToggleMainMenu()
    {
        if (mainMenu.activeSelf)
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
