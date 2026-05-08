using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HeadLookIK : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLookTargetProvider lookTargetProvider;

    [Header("IK Weights")]
    [SerializeField][Range(0f, 1f)] private float lookWeight = 1f;
    [SerializeField][Range(0f, 1f)] private float headWeight = 1f;
    [SerializeField][Range(0f, 1f)] private float clampWeight = 0.7f;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (lookTargetProvider == null)
            lookTargetProvider = GetComponentInParent<PlayerLookTargetProvider>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || lookTargetProvider == null) return;

        // Defensivo: garantiza que el provider tenga valores frescos en este frame.
        lookTargetProvider.UpdateFrame();

        float blend = lookTargetProvider.CurrentBlend;
        float bw = blend * lookWeight;
        if (bw < 0.0001f)
        {
            animator.SetLookAtWeight(0f);
            return;
        }

        animator.SetLookAtWeight(blend * lookWeight, 0f, blend * headWeight, blend * 0.5f, clampWeight);
        animator.SetLookAtPosition(lookTargetProvider.CurrentLookPosition);
    }
}
