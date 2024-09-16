using UnityEngine;

public class FallDetector : MonoBehaviour
{
    public delegate void TowerFallAction();
    public event TowerFallAction OnTowerFall;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Selectable"))
        {
            OnTowerFall?.Invoke();
        }
    }
}