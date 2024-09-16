using UnityEngine;
using TMPro;

public class IsFallenTextUpdate : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private TextMeshProUGUI textMeshPro;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    void Update()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (gameManager != null && textMeshPro != null)
        {
            bool isFallen = gameManager.IsTowerFallen();
            textMeshPro.text = $"Is Tower Fallen = {isFallen}";

            textMeshPro.color = isFallen ? Color.red : Color.green;
        }
    }
}