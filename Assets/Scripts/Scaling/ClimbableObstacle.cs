using UnityEngine;

public class ClimbableObstacle : MonoBehaviour
{
    public TraversalType traversalType;

    [Header("Traversal Points")]
    public Transform alignPoint;
    public Transform topPoint;
    public Transform exitPoint;
    public Transform hangPoint;
    public Transform dropPoint;

    [Header("Traversal Settings")]
    public float traversalDuration = 0.4f;
}
