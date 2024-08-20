using UnityEngine;
using TMPro;

public class IsFallenTextUpdate : MonoBehaviour
{
    private GameManager gameManager;
    private TextMeshProUGUI textMeshPro;

    void Start()
    {
        // Find the GameManager in the scene
        gameManager = FindObjectOfType<GameManager>();

        // Get the TextMeshPro component attached to this GameObject
        textMeshPro = GetComponent<TextMeshProUGUI>();

        // Initial update of the text
        UpdateText();
    }

    void Update()
    {
        // Continuously update the text to reflect the current state
        UpdateText();
    }

    private void UpdateText()
    {
        // Check if the GameManager and TextMeshPro components are found
        if (gameManager != null && textMeshPro != null)
        {
            // Update the text with the current status of the tower
            bool isFallen = gameManager.IsTowerFallen();
            textMeshPro.text = $"Is Tower Fallen = {isFallen}";

            // Change the text color based on the status
            if (isFallen)
            {
                textMeshPro.color = Color.red;
            }
            else
            {
                textMeshPro.color = Color.green;
            }
        }
    }
}