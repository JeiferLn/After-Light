using UnityEngine;

public class InteractableScanner
{
    private readonly Collider[] hitsBuffer;

    public InteractableScanner(int bufferSize = 32)
    {
        hitsBuffer = new Collider[Mathf.Max(1, bufferSize)];
    }

    public bool TryGetBest(
        Vector3 origin,
        Vector3 forwardHorizontal,
        float radius,
        float maxYawAngle,
        LayerMask layerFilter,
        out Vector3 worldPoint)
    {
        worldPoint = default;

        int hitCount = Physics.OverlapSphereNonAlloc(
            origin,
            radius,
            hitsBuffer,
            ~0,
            QueryTriggerInteraction.Collide);

        Collider best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = hitsBuffer[i];
            if (col == null) continue;
            if (!PassesLayerFilter(layerFilter, col.gameObject.layer)) continue;

            Vector3 targetPoint = col.bounds.center;
            Vector3 toTarget = targetPoint - origin;
            float dist = toTarget.magnitude;
            if (dist < 1e-4f || dist > radius) continue;

            Vector3 toH = toTarget;
            toH.y = 0f;
            float hLen = toH.magnitude;
            if (hLen < 1e-4f) continue;

            toH /= hLen;
            float yaw = Vector3.Angle(forwardHorizontal, toH);
            if (yaw > maxYawAngle) continue;

            float score = yaw + dist * 0.01f;
            if (score < bestScore)
            {
                bestScore = score;
                best = col;
            }
        }

        if (best == null) return false;

        worldPoint = best.bounds.center;
        return true;
    }

    private static bool PassesLayerFilter(LayerMask layerFilter, int layer)
    {
        if (layerFilter.value == 0) return true;
        return (layerFilter.value & (1 << layer)) != 0;
    }
}
