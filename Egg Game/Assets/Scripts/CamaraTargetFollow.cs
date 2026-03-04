using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 1f, 0f);

    void LateUpdate()
    {
        if (!player) return;
        transform.position = player.position + offset;
    }
}