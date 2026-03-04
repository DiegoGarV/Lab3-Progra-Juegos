using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        UIManager.Instance?.RegisterHit();
    }
}