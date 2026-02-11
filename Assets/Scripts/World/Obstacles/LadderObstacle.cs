using System.Collections.Generic;
using UnityEngine;

public class LadderObstacle : MonoBehaviour
{
    [Header("Ladder Points")]
    public List<Transform> startPoints;
    public List<Transform> endPoints;
    public float traversalDuration = 0.4f;
}
