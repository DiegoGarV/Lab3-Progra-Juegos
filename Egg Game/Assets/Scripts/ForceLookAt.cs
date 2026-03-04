using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class ForceLookAt : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (!target) return;
        transform.LookAt(target.position);
    }
}