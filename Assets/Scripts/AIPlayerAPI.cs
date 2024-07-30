using UnityEngine;

public class AIPlayerAPI : MonoBehaviour
{
    public enum PieceType
    {
        Yellow,
        Blue,
        Green
    }

    [Header("AI Settings")]
    public int levelSelect = 0;
    public PieceType pieceSelect = PieceType.Yellow;
    public bool performMove = false;
}
