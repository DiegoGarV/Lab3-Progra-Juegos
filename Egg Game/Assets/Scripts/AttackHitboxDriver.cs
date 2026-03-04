using UnityEngine;

public class AttackHitboxDriver : MonoBehaviour
{
    [SerializeField] private Collider attackHitbox;

    public void EnableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = true;
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
    }
}