using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

public class MapNodeGizmos : MonoBehaviour
{

    List<MapNode> mapNodes;
    List<Vector3> linePoints;
    public void OnDrawGizmos() {
        linePoints.Clear();
        mapNodes = FindObjectsOfType<MapNode>().ToList<MapNode>();
        Debug.Log(mapNodes);
        foreach (var mapNode in mapNodes)
        {
            string nodeID = mapNode.GetID();
            List<string> connectedIDs = mapNode.GetConnectedIDs();
            Handles.Label(mapNode.transform.position, nodeID);
            foreach (var connect in mapNodes)
            {
                if (connectedIDs.Contains(connect.GetID()))
                {
                    linePoints.Add(mapNode.transform.position);
                    linePoints.Add(connect.transform.position);
                }
            }
        }
        Gizmos.DrawLineList(linePoints.ToArray());
    }
}