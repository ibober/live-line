using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Navigation data cache service base class.
/// Inherit this class and use created with <see cref="CreateAssetMenuAttribute"/> asset in <see cref="PathFinder"/> component.
/// </summary>
public abstract class NavDataCacher : ScriptableObject
{
    public const string NavDataFileExtension = ".nav";

    internal abstract void Set(string siteName, NavMeshData navData);

    internal abstract bool TryGet(string siteName, out NavMeshData navData);

    internal abstract Task SetAsync(string siteName, NavMeshData navData);

    internal abstract Task<NavMeshData> GetAsync(string siteName);
}