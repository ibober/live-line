using UnityEngine;

internal struct NavigationInstruction
{
    public string Active { get; set; }

    public string Next { get; set; }
}

internal static class InstructionsFactory
{
    public static string[] GetInstruction(Vector3 previousPosition, Vector3 currentPosition, Vector3 nextPosition)
    {

        return new string[] { instructions[0], instructions[3] };
    }

    private static string[] instructions =
    {
        "Go stright",
        "Go stright for {0} meter{1}",
        "Turn left",
        "Turn right",
        "Go downstairs",
        "Go upstairs",
        "Pass the door stright",
        "Pass {0} doors stright",
        "Pass the door to the left",
        "Pass the door to the right",
    };
}