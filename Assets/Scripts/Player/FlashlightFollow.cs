using UnityEngine;

public class FlashlightFollow : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private LookController lookController;

    void LateUpdate()
    {
        if (lookController == null) return;

        Vector3 dir = (lookController.CurrentLookPosition - transform.position).normalized;

        Quaternion targetRot = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotationSpeed
        );
    }
}