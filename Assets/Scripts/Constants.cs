using Unity.AI.Navigation;

/// <summary>
/// *all dimensions are in meters.
/// </summary>
public struct Constants
{
    public struct Path
    {
        /// <summary>
        /// Maximum distance from source or destination points to the <see cref="NavMeshSurface"/>.
        /// <para>If point is outside this range path won't be calculated.</para>
        /// </summary>
        public const float MaxDistanceToNavSerface = 50f;

        /// <summary>
        /// Width of the path.
        /// </summary>
        public const float Width = 0.3f;

        /// <summary>
        /// Rise path above the <see cref="NavMeshSurface"/> so it is not overlapped with floors.
        /// </summary>
        public const float Elevation = 0.3f;

        public const string EvacuationLineMaterialPath = "Materials/EvacuationLine";
    }

    public struct Instructions
    {
        /// <summary>
        /// Minimum elevation to travel to treat it as going stairs.
        /// </summary>
        public const float MinStairElevation = 0.47f; // 3 standard steps 150 mm high + epsilon.

        /// <summary>
        /// If elevation per horizontal displacement is less than this ratio - we are not going any stairs.
        /// <para>1 meter rise for 2 meters stright movement.</para>
        /// </summary>
        public const float MinStairsAngle = 26.565f;

        /// <summary>
        /// Minimum angle to treat direction change as a turn. In degrees.
        /// </summary>
        public const float TurnAngleThreshold = 27f;

        /// <summary>
        /// Minimum angle to treat direction change as a backturn. In degrees.
        /// </summary>
        public const float BackturnAngleThreshold = 144f;

        /// <summary>
        /// Show more details on the span of the path if it's longer than the value.
        /// </summary>
        public const float ShowMoreDetailsMinSpan = 3f;

        /// <summary>
        /// Distance to the key point at which next instruction must start to appear.
        /// </summary>
        public const float TransitionDistance = 2f;

        /// <summary>
        /// If span is shorter than <see cref="TransitionDistance"/> - show next instruction at this part of the span.
        /// </summary>
        public const float TransitionRation = 0.5f;

        /// <summary>
        /// Actual distance to the key point at which next instruction must satrt to appear for the current span.
        /// </summary>
        public static float ActualTransitionDistance(float span)
        {
            return span > TransitionDistance ? TransitionDistance : (span * TransitionRation);
        }
    }
}