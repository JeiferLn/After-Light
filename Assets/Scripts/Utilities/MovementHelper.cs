using System;
using System.Collections;
using UnityEngine;

public static class MovementHelper
{
    public static IEnumerator MoveTo(
        Transform transform,
        Action<Vector3> applyDelta,
        Vector3 target,
        float duration
    )
    {
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 next = Vector3.Lerp(start, target, elapsed / duration);
            Vector3 delta = next - transform.position;
            applyDelta(delta);
            yield return null;
        }
    }
}
