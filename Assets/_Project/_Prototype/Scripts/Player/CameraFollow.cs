using UnityEngine;

/// <summary>
/// Smoothly follows a target while preserving the camera's configured Z depth.
/// The target is assigned in the scene so this remains reusable for future areas.
/// </summary>
public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField, Min(0.01f)]
    private float smoothTime = 0.12f;

    private Vector3 velocity;
    private float targetDepth;

    private void Awake()
    {
        targetDepth = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, targetDepth);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
