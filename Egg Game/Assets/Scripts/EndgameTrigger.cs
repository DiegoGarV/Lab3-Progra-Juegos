using UnityEngine;

public class EndGameTrigger : MonoBehaviour
{
    private bool _done;

    void OnTriggerEnter(Collider other)
    {
        if (_done) return;
        if (!other.CompareTag("Player")) return;

        _done = true;

        if (UIManager.Instance != null)
            UIManager.Instance.TriggerWin();
    }
}