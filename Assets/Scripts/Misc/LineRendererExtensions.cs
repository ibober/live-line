using UnityEngine;

internal static class LineRendererExtensions
{
    /// <summary>
    /// Calculates the total length of the <see cref="LineRenderer"/> by summing up the distances between its points.
    /// </summary>
    /// <param name="lineRenderer">The LineRenderer to calculate the length for.</param>
    /// <returns>The total length of the LineRenderer.</returns>
    public static float GetTotalLength(this LineRenderer lineRenderer)
    {
        if (lineRenderer.positionCount < 2)
            return 0f;

        var totalLength = 0f;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            var start = lineRenderer.GetPosition(i);
            var end = lineRenderer.GetPosition(i + 1);
            totalLength += Vector3.Distance(start, end);
        }

        return totalLength;
    }

    /// <summary>
    /// Calculates the total squared length of the <see cref="LineRenderer"/> by summing up the magnitudes between its points.
    /// <para>This value can be taken as a progress measurement while navigating the pass.</para>
    /// </summary>
    /// <param name="lineRenderer"></param>
    /// <returns></returns>
    public static float GetTotalSqrMagnitude(this LineRenderer lineRenderer)
    {
        if (lineRenderer.positionCount < 2)
            return 0f;

        var totalMagnitude = 0f;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            var start = lineRenderer.GetPosition(i);
            var end = lineRenderer.GetPosition(i + 1);
            totalMagnitude += (start - end).sqrMagnitude;
        }

        return totalMagnitude;
    }

    /// <summary>
    /// Finds the closest point on the <see cref="LineRenderer"/> to the specified position.
    /// </summary>
    /// <param name="lineRenderer">The <see cref="LineRenderer"/> to search.</param>
    /// <param name="position">The position to find the closest point to.</param>
    /// <returns>The closest point on the <see cref="LineRenderer"/>.</returns>
    public static Vector3 GetClosestPoint(this LineRenderer lineRenderer, Vector3 position)
    {
        if (lineRenderer.positionCount == 0)
            throw new System.InvalidOperationException("LineRenderer has no points.");

        var closestPoint = lineRenderer.GetPosition(0);
        var closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            var start = lineRenderer.GetPosition(i);
            var end = lineRenderer.GetPosition(i + 1);

            var segmentClosestPoint = GetClosestPointOnSegment(start, end, position);
            var segmentDistanceSqr = (segmentClosestPoint - position).sqrMagnitude;
            if (segmentDistanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = segmentDistanceSqr;
                closestPoint = segmentClosestPoint;
            }
        }

        return closestPoint;
    }

    private static Vector3 GetClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 position)
    {
        var segmentDirection = end - start;
        var segmentLengthSqr = segmentDirection.sqrMagnitude;
        if (segmentLengthSqr == 0)
            return start;

        var t = Vector3.Dot(position - start, segmentDirection) / segmentLengthSqr;
        t = Mathf.Clamp01(t); // Clamp to ensure the point is on the segment.

        return start + t * segmentDirection;
    }
}
