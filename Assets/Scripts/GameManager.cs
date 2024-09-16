using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private HumanPlayer humanPlayer;
    [SerializeField] private GameObject fallDetectorsParent;
    [SerializeField] private TMP_Text playerTurnDisplay;
    [SerializeField] private MainMenuManager mainMenuManager;

    private bool isTowerFallen = false;
    private AiSelector aiSelector;
    private CommandDispatcher commandDispatcher;
    private CameraController cameraController;

    public PlayerType currentPlayerType;
    public int currentPlayerIndex;
    public int currentRoundNum;

    private void Start()
    {
        aiSelector = FindObjectOfType<AiSelector>();
        if (aiSelector == null)
        {
            Debug.LogError("AiSelector component not found in the scene.");
        }

        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController component not found in the scene.");
        }

        commandDispatcher = FindObjectOfType<CommandDispatcher>();
        if (commandDispatcher == null)
        {
            Debug.LogError("CommandDispatcher component not found in the scene.");
        }

        SetupFallDetectors();
    }

    private void SetupFallDetectors()
    {
        if (fallDetectorsParent != null)
        {
            foreach (Transform detector in fallDetectorsParent.transform)
            {
                var fallDetector = detector.GetComponent<FallDetector>();
                if (fallDetector != null)
                {
                    fallDetector.OnTowerFall += () => HandleTowerFall(fallDetector);
                }
            }
        }
    }

    public void PlayerTurn(int playerType, int playerIndex, int roundNum)
    {
        playerTurnDisplay.text = $"Now playing PlayerType: {((PlayerType)playerType)}, PlayerIndex: {playerIndex + 1}, Round: {roundNum}";
        currentPlayerType = (PlayerType)playerType;
        currentPlayerIndex = playerIndex;
        currentRoundNum = roundNum;

        if ((PlayerType)playerType == PlayerType.HUMAN)
        {
            StartCoroutine(HandleHumanTurn(playerIndex));
        }
        else
        {
            cameraController.Start();
            Debug.Log("AI Player turn. Waiting for Python command to remove a piece.");
        }
    }

    private IEnumerator HandleHumanTurn(int playerIndex)
    {
        yield return StartCoroutine(humanPlayer.HandleTurn());

        if (aiSelector != null)
        {
            // This delay fixes red selected piece appearing in screenshot
            yield return new WaitForSeconds(0.25f);
            aiSelector.UpdateAfterHumanTurn();
        }
    }

    public void ToggleMenu()
    {
        mainMenuManager.ToggleMainMenu();
    }

    private void HandleTowerFall(FallDetector fallDetector)
    {
        var meshRenderer = fallDetector.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }

        Debug.Log("The tower has fallen!");
        isTowerFallen = true;
        playerTurnDisplay.text = $"Player Collapsed the tower: PlayerType: {((PlayerType)currentPlayerType)}, PlayerIndex: {currentPlayerIndex + 1}, Round: {currentRoundNum}";
    }

    public bool IsTowerFallen()
    {
        return isTowerFallen;
    }

    public void RestartGame()
    {
        var pythonListener = FindObjectOfType<PythonListener>();
        if (pythonListener != null)
        {
            pythonListener.StopListener();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void ResetIsTowerFallen()
    {
        isTowerFallen = false;
    }
}
