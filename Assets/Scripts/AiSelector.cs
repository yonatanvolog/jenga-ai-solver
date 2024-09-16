using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AiSelector : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material selectionMaterial;
    [SerializeField] private float highlightTime = 1.0f;
    [SerializeField] private float selectedTime = 1.0f;
    [SerializeField] private bool delayEnabled = true;
    [SerializeField] private AIPlayerAPI aiPlayer;

    [SerializeField] private GameObject jengaTower;
    [SerializeField] private GameObject prevJengaTower;

    [SerializeField] private int updateFrameInterval = 10;
    [SerializeField] private CommandDispatcher commandDispatcher;

    private bool pieceSelected;
    private bool lastActionWasRevert = false;
    private float prevAverageOfMaxAngles = 0f;
    private float prevMaxOfMaxAngles = 0f;
    private int towerInstanceCounter = 0;
    private int frameCounter = 0;

    [System.Serializable]
    public class JengaLevel
    {
        public Transform[] pieces = new Transform[3];
    }
    [SerializeField] private JengaLevel[] jengaTowerLevels;

    private Material originalMaterialHighlight;
    private Material originalMaterialSelection;

    private Screenshot screenshot;
    private GameManager gameManager;
    private FallDetectModifier fallDetectModifier;

    private Dictionary<Transform, float> pieceMaxTiltAngles = new Dictionary<Transform, float>();

    void Start()
    {
        commandDispatcher = FindObjectOfType<CommandDispatcher>();
        if (commandDispatcher == null)
        {
            Debug.LogError("CommandDispatcher component not found in the scene.");
        }

        screenshot = FindObjectOfType<Screenshot>();
        if (screenshot == null)
        {
            Debug.LogError("Screenshot component not found in the scene.");
        }
        fallDetectModifier = FindObjectOfType<FallDetectModifier>();

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager component not found in the scene.");
        }

        ParseJengaTower();
        ResetCubesStartingAngles();
        StartCoroutine(UpdateMaxTiltAnglesRoutine());
    }

    public void UpdateAfterHumanTurn()
    {
        ParseJengaTower();
        StartCoroutine(TakeScreenshotAfterFrame());
    }

    private void ParseJengaTower()
    {
        StartCoroutine(ParseJengaTowerWithDelay());
    }

    private IEnumerator ParseJengaTowerWithDelay()
    {
        yield return new WaitForFixedUpdate();

        jengaTowerLevels = new JengaLevel[jengaTower.transform.childCount];

        if (jengaTower == null)
        {
            Debug.LogError("Jenga tower not assigned.");
            yield break;
        }

        int levelIndex = 0;
        foreach (Transform level in jengaTower.transform)
        {
            JengaLevel jengaLevel = new JengaLevel();
            for (int i = 0; i < 3; i++)
            {
                jengaLevel.pieces[i] = null;
            }

            foreach (Transform piece in level)
            {
                string pieceName = piece.name;
                if (pieceName.StartsWith("0"))
                {
                    jengaLevel.pieces[0] = piece;
                }
                else if (pieceName.StartsWith("1"))
                {
                    jengaLevel.pieces[1] = piece;
                }
                else if (pieceName.StartsWith("2"))
                {
                    jengaLevel.pieces[2] = piece;
                }
            }

            jengaTowerLevels[levelIndex] = jengaLevel;
            levelIndex++;
        }
    }

    private async Task FlashSelectedPieceAsync(Transform pieceTransform, int flashTimes)
    {
        MeshRenderer pieceRenderer = pieceTransform.GetComponent<MeshRenderer>();

        if (pieceRenderer == null)
        {
            Debug.LogError("Selected piece does not have a MeshRenderer component.");
            return;
        }

        Material originalMaterial = pieceRenderer.material;

        for (int i = 0; i < flashTimes; i++)
        {
            pieceRenderer.material = highlightMaterial;
            await Task.Delay((int)(highlightTime * 1000));

            pieceRenderer.material = selectionMaterial;
            await Task.Delay((int)(selectedTime * 1000));
        }

        pieceRenderer.material = originalMaterial;
    }

    public int GetNumOfBlocksInLevel(int level)
    {
        if (level < 0 || level >= jengaTowerLevels.Length)
        {
            Debug.LogError("Invalid level index.");
            return 0;
        }

        for (int i = 0; i < jengaTowerLevels.Length; i++)
        {
            int reversedIndex = jengaTowerLevels.Length - 1 - i;
            int numBlocks = jengaTowerLevels[reversedIndex].pieces.Length;
            Debug.Log($"Level {i} (Reversed Index {reversedIndex}): {numBlocks} blocks");
        }

        int reversedLevelIndex = jengaTowerLevels.Length - 1 - level;

        if (reversedLevelIndex < 0 || reversedLevelIndex >= jengaTowerLevels.Length)
        {
            Debug.LogError("Invalid reversed level index.");
            return 0;
        }

        int blockCount = 0;
        foreach (var piece in jengaTowerLevels[reversedLevelIndex].pieces)
        {
            if (piece != null) blockCount++;
        }

        return blockCount;
    }

    private async void PerformMove()
    {
        int selectedLevel = aiPlayer.levelSelect;
        string selectedColor = ColorMapping.GetColor((int)aiPlayer.pieceSelect);
        if (aiPlayer.levelSelect < 0 || aiPlayer.levelSelect >= jengaTowerLevels.Length)
        {
            Debug.LogError("Invalid levelSelect value.");
            return;
        }
        int reversedLevelIndex = jengaTowerLevels.Length - 1 - aiPlayer.levelSelect;
        if (reversedLevelIndex < 0 || reversedLevelIndex >= jengaTowerLevels.Length)
        {
            Debug.LogError("Invalid reversed level index.");
            return;
        }
        JengaLevel levelList = jengaTowerLevels[reversedLevelIndex];
        if (aiPlayer.pieceSelect < AIPlayerAPI.PieceType.Yellow || aiPlayer.pieceSelect > AIPlayerAPI.PieceType.Green)
        {
            Debug.LogError("Invalid pieceSelect value.");
            return;
        }
        int pieceIndex = (int)aiPlayer.pieceSelect;
        Transform pieceTransform = levelList.pieces[pieceIndex];
        pieceSelected = true;

        await FlashSelectedPieceAsync(pieceTransform, 3);
        StartCoroutine(TakeScreenshotAfterFrame());

        HandlePieceMoveQuick(pieceTransform);
        commandDispatcher.DispatchFinishedMove(selectedLevel, selectedColor);
    }

    private IEnumerator TakeScreenshotAfterFrame()
    {
        yield return new WaitForEndOfFrame();

        if (screenshot != null)
        {
            screenshot.TakeScreenshot();
        }
    }

    private void HandlePieceMoveQuick(Transform pieceTransform)
    {
        Destroy(pieceTransform.gameObject);
        ResetCubesStartingAngles();
    }
    
    // This method was used for GSBAS, but we decided not to use it and this method was depicted to improve performance
    private void SaveCurrentState()
    {
        if (jengaTower == null)
        {
            Debug.LogError("Jenga tower is null. Cannot save current state.");
            return;
        }

        jengaTower.SetActive(false);

        if (prevJengaTower != null && prevJengaTower != jengaTower)
        {
            Destroy(prevJengaTower);
        }

        prevJengaTower = Instantiate(jengaTower);
        towerInstanceCounter++;
        prevJengaTower.name = jengaTower.name + "_" + towerInstanceCounter;
        prevJengaTower.SetActive(false);

        jengaTower.SetActive(true);

        prevAverageOfMaxAngles = GetAverageOfMaxAngles();
        prevMaxOfMaxAngles = GetMaxOfMaxAngles();

        lastActionWasRevert = false;
    }
    
    // This method was used for GSBAS, but we decided not to use it and this method was depicted to improve performance
    public void RevertStep()
    {
        if (prevJengaTower == null)
        {
            Debug.LogWarning("No previous state to revert to.");
            return;
        }

        if (jengaTower != null)
        {
            Destroy(jengaTower);
            print("Current tower destroyed.");
        }
        else
        {
            Debug.LogWarning("No current Jenga tower to destroy.");
        }

        jengaTower = prevJengaTower;
        jengaTower.SetActive(true);
        prevJengaTower = null;

        ParseJengaTower();
        ResetCubesStartingAngles();
        lastActionWasRevert = true;

        if (gameManager != null)
        {
            gameManager.ResetIsTowerFallen();
        }

        if (fallDetectModifier != null)
        {
            fallDetectModifier.ResetFallDetectors();
        }
    }

    private void ResetCubesStartingAngles()
    {
        pieceMaxTiltAngles.Clear();

        foreach (var level in jengaTowerLevels)
        {
            foreach (var piece in level.pieces)
            {
                if (piece != null)
                {
                    float rollAngle = Mathf.DeltaAngle(0, piece.eulerAngles.x);
                    float pitchAngle = Mathf.DeltaAngle(0, piece.eulerAngles.z);
                    float currentTiltAngle = Mathf.Max(Mathf.Abs(rollAngle), Mathf.Abs(pitchAngle));

                    pieceMaxTiltAngles[piece] = currentTiltAngle;
                }
            }
        }
    }

    public float GetAverageOfMaxAngles()
    {
        if (lastActionWasRevert)
        {
            return prevAverageOfMaxAngles;
        }

        if (pieceMaxTiltAngles.Count == 0) return 0f;

        float totalMaxAngle = 0f;

        foreach (var maxAngle in pieceMaxTiltAngles.Values)
        {
            totalMaxAngle += maxAngle;
        }

        return totalMaxAngle / pieceMaxTiltAngles.Count;
    }

    public float GetMaxOfMaxAngles()
    {
        if (lastActionWasRevert)
        {
            return prevMaxOfMaxAngles;
        }

        if (pieceMaxTiltAngles.Count == 0) return 0f;

        float maxTiltAngle = 0f;

        foreach (var maxAngle in pieceMaxTiltAngles.Values)
        {
            if (maxAngle > maxTiltAngle)
            {
                maxTiltAngle = maxAngle;
            }
        }

        return maxTiltAngle;
    }

    private IEnumerator UpdateMaxTiltAnglesRoutine()
    {
        while (true)
        {
            if (frameCounter >= updateFrameInterval)
            {
                yield return StartCoroutine(UpdateMaxTiltAnglesForAllPiecesCoroutine());
                frameCounter = 0;
            }
            frameCounter++;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator UpdateMaxTiltAnglesForAllPiecesCoroutine()
    {
        foreach (var level in jengaTowerLevels)
        {
            foreach (var piece in level.pieces)
            {
                if (piece != null && pieceMaxTiltAngles.ContainsKey(piece))
                {
                    float rollAngle = Mathf.DeltaAngle(0, piece.eulerAngles.x);
                    float pitchAngle = Mathf.DeltaAngle(0, piece.eulerAngles.z);
                    float currentTiltAngle = Mathf.Max(Mathf.Abs(rollAngle), Mathf.Abs(pitchAngle));

                    pieceMaxTiltAngles[piece] = Mathf.Max(pieceMaxTiltAngles[piece], currentTiltAngle);
                }
            }

            yield return null;
        }
    }

    void Update()
    {
        if (!aiPlayer.performMove)
        {
            return;
        }

        PerformMove();
        aiPlayer.levelSelect = 0;
        aiPlayer.pieceSelect = AIPlayerAPI.PieceType.Yellow;
        aiPlayer.performMove = false;
    }
}
