using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Constants;

internal class NavigationInstructions : IEnumerable<NavigationInstruction>
{
    private static readonly string[] Hints =
    {
        // TODO Localize hints and convert to imperial system.
        "Go straight",
        "Go straight for {0} meters",
        "Turn left",
        "Turn right",
        "Turn around left shoulder", // index 4
        "Turn around right shoulder",
        "Go downstairs",
        "Go upstairs",
        "Pass the door stright",
        "Pass {1} doors stright", // index 9
        "Pass the door to the left",
        "Pass the door to the right",
        "Destination ahead",
        "Destination in {0} meters", // index 13
        "Arriving",
    };

    private static string DecideStraightText(float span) => span > Instructions.ShowMoreDetailsMinSpan ? Hints[1] : Hints[0];
    private static string DecideDestinationText(float span) => span > Instructions.ShowMoreDetailsMinSpan ? Hints[13] : Hints[12];

    private readonly LineRenderer path;
    private readonly List<GameObject> objectsOfInterest;
    private readonly Stack<NavigationInstruction> instructions;

    private float pathLength;

    /// <summary>
    /// Navigation instructions which one meets on his way along the <see cref="path"/>.
    /// </summary>
    /// <param name="path">Path from origin to the destination.</param>
    /// <param name="objectsOfInterest">E.g. doors.</param>
    /// <exception cref="InvalidOperationException">Thrown when path is invalid.</exception>
    public NavigationInstructions(LineRenderer path, List<GameObject> objectsOfInterest = null)
    {
        if (path.positionCount < 2)
        {
            throw new InvalidOperationException("Path can't consist of less than 2 points.");
        }

        this.path = path;
        this.objectsOfInterest = objectsOfInterest;
        instructions = new();
    }

    /// <summary>
    /// First instruction we meet in the sequence of similar ones.
    /// </summary>
    private NavigationInstruction remembered;

    public void Calculate()
    {
        // We'll go from finish to start accumulating distances, merging similar instructions.
        var nextPoint = Vector3.negativeInfinity;
        var currentPoint = path.GetPosition(path.positionCount - 1);
        var previousPoint = path.GetPosition(path.positionCount - 2);
        pathLength = Vector3.Distance(currentPoint, previousPoint);
        instructions.Push(GetInstruction(previousPoint, currentPoint, nextPoint, pathLength));
        for (int i = path.positionCount - 2; i > 1; i--)
        {
            nextPoint = path.GetPosition(i + 1);
            currentPoint = path.GetPosition(i);
            previousPoint = path.GetPosition(i - 1);
            pathLength += Vector3.Distance(currentPoint, nextPoint);
            var candidate = GetInstruction(previousPoint, currentPoint, nextPoint, pathLength);
            if (candidate.DistanceSpan < Instructions.TransitionDistance)
            {
                //instructions.Peek().ApplicablePoint = candidate.ApplicablePoint;
                //instructions.Peek().DistanceSpan += candidate.DistanceSpan;
                continue;
            }
            if (Merge(remembered, candidate, out var instruction))
            {

            }
            else
            {
                instructions.Push(instruction);
                remembered = instruction;
            }
        }
        nextPoint = path.GetPosition(1);
        currentPoint = path.GetPosition(0);
        previousPoint = Vector3.positiveInfinity;
        pathLength += Vector3.Distance(currentPoint, nextPoint);
        instructions.Push(GetInstruction(previousPoint, currentPoint, nextPoint, pathLength));
    }

    /// <summary>
    /// Get instruction for the current span of a path.
    /// </summary>
    /// <param name="previousP">Previous key point on the path.</param>
    /// <param name="currentP">Current key point on the path.</param>
    /// <param name="nextP">Next key point on the path.</param>
    /// <param name="distanceLeft">Total distance left till the end of the path.</param>
    /// <returns><see cref="NavigationInstruction"/> for current key point on the path.</returns>
    private NavigationInstruction GetInstruction(Vector3 previousP, Vector3 currentP, Vector3 nextP, float distanceLeft)
    {
        float span;
        if (nextP == Vector3.negativeInfinity)
        {
            // Arrived.
            span = Vector3.Distance(currentP, previousP);
            return new NavigationInstruction(DecideDestinationText(span), distanceLeft, span, currentP);
        }
        else if (previousP == Vector3.positiveInfinity)
        {
            // In the very beginning of the route.
            span = Vector3.Distance(currentP, nextP);
            return new NavigationInstruction(DecideStraightText(span), distanceLeft, span, currentP);
        }

        span = Vector3.Distance(currentP, nextP);
        var elevation = nextP.y - currentP.y;
        if (elevation < -Instructions.MinStairElevation)
        {
            // Going downstairs.
            return new NavigationInstruction(Hints[6], distanceLeft, span, currentP);
        }
        else if (elevation > Instructions.MinStairElevation)
        {
            // Upstairs.
            return new NavigationInstruction(Hints[7], distanceLeft, span, currentP);
        }

        var incomingDirection = currentP - previousP;
        var outgoingDirection = nextP - currentP;

        incomingDirection.y = 0;
        outgoingDirection.y = 0;

        var angle = Vector3.Angle(incomingDirection, outgoingDirection);
        var cross = Vector3.Cross(incomingDirection, outgoingDirection);
        if (angle > Instructions.BackturnAngleThreshold)
        {
            if (cross.y < 0)
                // Left
                return new NavigationInstruction(Hints[4], distanceLeft, span, currentP);
            else if (cross.y > 0)
                // Right
                return new NavigationInstruction(Hints[5], distanceLeft, span, currentP);
        }
        else if (angle > Instructions.TurnAngleThreshold)
        {
            if (cross.y < 0)
                // Left
                return new NavigationInstruction(Hints[2], distanceLeft, span, currentP);
            else if (cross.y > 0)
                // Right
                return new NavigationInstruction(Hints[3], distanceLeft, span, currentP);
        }

        return new NavigationInstruction(DecideStraightText(span), distanceLeft, span, currentP);
    }

    private bool CheckForIntersections(Vector3 currentPosition, Vector3 nextPosition, out Vector3 intersection)
    {
        intersection = Vector3.negativeInfinity;

        // TODO Check if span intersects doors or is close to other objects of interest.
        var objects = objectsOfInterest.Where(go =>
        {
            // TODO Has to add collider or change logic to rely on bounds, position or smth.
            var direction = nextPosition - currentPosition;
            var ray = new Ray(currentPosition, direction);
            return Physics.Raycast(ray, out var hit, direction.magnitude); // hit.transform.gameObject
        });

        return false;
    }

    // TODO Implement method to merge navigation instructions.
    private bool Merge(NavigationInstruction next, NavigationInstruction first, out NavigationInstruction accumulated)
    {
        accumulated = first;
        if (first == null)
            return false;

        if (first.Text.StartsWith(Hints[0]))
        {

        }
        else if (first.Text.StartsWith("Turn left"))
        {

        }

        return false;
    }

    internal void Log()
    {
        var route = new StringBuilder();
        foreach (var i in instructions)
        {
            route.AppendLine($"{i.DistanceLeft} m - {i.UpdatedText()}");
        }
        Debug.Log($"Total distance to travel: {pathLength} meters" +
            $"\nNavigation instructions:" +
            $"\n{route.ToString()}");
    }

    #region IEnumerable and [] accessors

    /// <param name="distanceLeft">Total distance left till the end of the path.</param>
    /// <returns>Text of the actual instruction.</returns>
    public string this[float distanceLeft]
    {
        get
        {
            var closestInstruction = instructions
                .OrderBy(i => distanceLeft - i.DistanceLeft)
                .First(i => distanceLeft - i.DistanceLeft > 0);
            return closestInstruction.UpdatedText(distanceLeft);
        }
    }

    /// <param name="actualPosition">Current position closer to the path.</param>
    /// <returns>Text of the actual instruction.</returns>
    public string this[Vector3 actualPosition]
    {
        get
        {
            var closestInstruction = instructions
                .OrderBy(i => (actualPosition - i.ApplicablePoint).sqrMagnitude)
                .First();
            return closestInstruction.UpdatedText(actualPosition);
        }
    }

    public IEnumerator<NavigationInstruction> GetEnumerator()
    {
        return instructions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion IEnumerable
}
