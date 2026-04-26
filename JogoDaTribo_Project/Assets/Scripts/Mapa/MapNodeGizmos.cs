#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MapNodeGizmos : MonoBehaviour
{
    private List<MapNode> mapNodes;
    private List<Vector3> linePoints = new List<Vector3>();

    public void OnDrawGizmos()
    {
        linePoints.Clear();
        mapNodes = FindObjectsOfType<MapNode>().ToList();

        foreach (var mapNode in mapNodes)
        {
            string nodeID = mapNode.GetID();
            List<string> connectedIDs = mapNode.GetConnectedIDs();
            Handles.Label(mapNode.transform.position, nodeID);

            foreach (var other in mapNodes)
            {
                if (connectedIDs.Contains(other.GetID()))
                {
                    linePoints.Add(mapNode.transform.position);
                    linePoints.Add(other.transform.position);
                }
            }
        }

        Gizmos.DrawLineList(linePoints.ToArray());
    }
}
#endif
