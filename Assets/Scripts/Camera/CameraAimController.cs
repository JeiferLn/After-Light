using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

public class CameraAimController : MonoBehaviour
{
    [SerializeField]
    private CinemachineThirdPersonFollow thirdPersonFollow;

    [SerializeField]
    private Vector3 normalPos = new Vector3(0.798f, 2.481f, 0.472f);
    [SerializeField]
    private Vector3 aimPos = new Vector3(0.697f, 2.295f, 1.664f);
    [SerializeField]
    private float duration = 0.2f;
    private bool isAiming = false;
    private Tween currentTween;

    public void SetTargetPosition(bool isAiming)
    {
        if (currentTween != null)
        {
            currentTween.Kill();
        }

        this.isAiming = isAiming;
    }

    void Update()
    {
        if (isAiming)
        {
            MoveTo(aimPos);
            thirdPersonFollow.Damping.z = 0.3f;
        }
        else
        {
            MoveTo(normalPos);
            thirdPersonFollow.Damping.z = 1f;
        }
    }

    void MoveTo(Vector3 targetPos)
    {
        currentTween?.Kill();

        currentTween = transform.DOLocalMove(targetPos, duration)
                    .SetEase(Ease.OutSine);
    }
}
