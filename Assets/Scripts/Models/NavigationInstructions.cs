using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationInstructions : IEnumerable<string>
{
    private LineRenderer line;
    private Dictionary<float, string> instructions;

    public NavigationInstructions(LineRenderer path, List<GameObject> framedObjects = null)
    {
        line = path;
        instructions = new();

    }

    public Dictionary<float, string> GetInstructions(List<GameObject> framedObjects)
    {
        return instructions;
    }

    public Dictionary<float, string> UpdateInstructions(LineRenderer path = null, List<GameObject> framedObjects = null)
    {
        if (path != null)
        {
            line = path;
        }


        return instructions;
    }

    public string GetCurrent(float progress, out string next)
    {
        next = string.Empty;
        return string.Empty;
    }

    public string GetNext(float progress)
    {
        return string.Empty;
    }

    public string this[float progress]
    {
        get
        {
            var closestKey = instructions.Keys.OrderBy(key => Math.Abs(key - progress)).First();
            return instructions[closestKey];
        }
        set => instructions[progress] = value;
    }

    #region IEnumerable

    public IEnumerator<string> GetEnumerator()
    {
        return instructions.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion IEnumerable
}
