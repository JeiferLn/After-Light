using Unity.Cinemachine;
using UnityEngine;

public class CameraLookDirection : MonoBehaviour
{
    [SerializeField]
    [Range(1f, 20f)]
    private float smoothSpeed = 8f;

    private CinemachinePositionComposer composer;
    private float targetScreenX;

    void Start()
    {
        composer = GetComponent<CinemachinePositionComposer>();
        targetScreenX = composer.Composition.ScreenPosition.x;
    }

    void Update()
    {
        var pos = composer.Composition.ScreenPosition;
        pos.x = Mathf.Lerp(pos.x, targetScreenX, smoothSpeed * Time.deltaTime);
        composer.Composition.ScreenPosition = pos;
    }

    public void LookDirection(bool isRightDirection)
    {
        targetScreenX = isRightDirection ? -0.15f : 0.15f;
    }
}
