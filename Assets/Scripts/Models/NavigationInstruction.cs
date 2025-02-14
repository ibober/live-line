using UnityEngine;

internal class NavigationInstruction
{
    private bool canUpdate;

    public NavigationInstruction(string text, float remainder, float span, Vector3 applicablePoint)
    {
        canUpdate = text.Contains("{0}");
        Text = text;
        DistanceLeft = remainder;
        DistanceSpan = span;
        ApplicablePoint = applicablePoint;
    }

    /// <summary>
    /// Navigation instruction text.
    /// </summary>
    public string Text { get; private set; }

    public string Notes { get; set; }

    /// <summary>
    /// Distance left to the end of the path when instruction appears.
    /// </summary>
    public float DistanceLeft { get; }

    /// <summary>
    /// The distance along which instruction is valid.
    /// </summary>
    public float DistanceSpan { get; private set; }

    /// <summary>
    /// Point on the path until which instruction is applicable.
    /// </summary>
    public Vector3 ApplicablePoint { get; private set; }

    public string UpdatedText(Vector3 actualPosition)
    {
        if (!canUpdate)
            return Text;

        var remainder = Vector3.Distance(actualPosition, ApplicablePoint) - Constants.Instructions.ActualTransitionDistance(DistanceSpan);
        return string.Format(Text, remainder);
    }

    public string UpdatedText(float? actualDistanceLeft = null)
    {
        if (!canUpdate)
            return Text;

        actualDistanceLeft ??= DistanceLeft;
        var remainder = DistanceSpan - (DistanceLeft - actualDistanceLeft.Value);
        return string.Format(Text, remainder);
    }

    public void ExtendTo(NavigationInstruction nextInstruction, InheritInstruction inheritInstruction = InheritInstruction.Longer)
    {
        if (inheritInstruction == InheritInstruction.Longer)
            Text = nextInstruction.Text.Length > Text.Length ? nextInstruction.Text : Text;
        else if (inheritInstruction == InheritInstruction.Another)
            Text = nextInstruction.Text;
        canUpdate = Text.Contains("{0}");
        Notes += nextInstruction.Notes;
        DistanceSpan += nextInstruction.DistanceSpan;
        ApplicablePoint = nextInstruction.ApplicablePoint;
    }

    public void PrependTo(NavigationInstruction previousInstruction, InheritInstruction inheritInstruction = InheritInstruction.Longer)
    {
        if (inheritInstruction == InheritInstruction.Longer)
            Text = previousInstruction.Text.Length > Text.Length ? previousInstruction.Text : Text;
        else if (inheritInstruction == InheritInstruction.Another)
            Text = previousInstruction.Text;
        canUpdate = Text.Contains("{0}");
        Notes = previousInstruction.Notes + Notes;
        DistanceSpan += previousInstruction.DistanceSpan;
    }
}

internal enum InheritInstruction
{
    Longer = 0,
    Current = 1,
    Another = 2,
}