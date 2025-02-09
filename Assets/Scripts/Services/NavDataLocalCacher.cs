using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = nameof(NavDataLocalCacher), menuName = "Services/" + nameof(NavDataLocalCacher), order = 1)]
internal class NavDataLocalCacher : NavDataCacher
{
    internal override void Set(string siteName, NavMeshData navData)
    {
        try
        {
            var fileName = siteName + NavDataFileExtension;
            var formatter = new BinaryFormatter();
            using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, navData);
        }
        catch (Exception x)
        {
            Debug.LogError(x.ToString());
        }
    }

    internal override bool TryGet(string siteName, out NavMeshData navData)
    {
        navData = null;
        try
        {
            var fileName = siteName + NavDataFileExtension;
            var formatter = new BinaryFormatter();
            using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            navData = (NavMeshData)formatter.Deserialize(stream);
            return true;
        }
        catch (Exception x)
        {
            Debug.LogError(x.ToString());
        }

        return false;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    internal override async Task SetAsync(string siteName, NavMeshData navData)
    {
        // TODO Implement async navigation data caching.
    }

    internal override async Task<NavMeshData> GetAsync(string siteName)
    {
        // TODO Implement async navigation data caching.

        return default;
    }
}