using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNodeSave
{
    MapNode.MapNodeTypes nodeType;
    string nodeID;
    List<string> connectedIDs = new List<string>();
    Vector3 position;

    public void SaveNode(MapNode node)
    {
        SetMapNodeType(node.GetNodeType());
        SetID(node.GetID());
        SetConnectedIDs(node.GetConnectedIDs());
        SetPosition(node.transform.localPosition);
    }

    public void SetMapNodeType(MapNode.MapNodeTypes nodeType)
    {
        this.nodeType = nodeType;
    }
    public void SetID(string nodeID)
    {
        this.nodeID = nodeID;
    }
    public void SetConnectedIDs(List<string> connectedIDs)
    {
        this.connectedIDs = connectedIDs;
    }
    public void SetPosition(Vector3 position)
    {
        this.position = position;
    }

    public MapNode.MapNodeTypes GetNodeType() => nodeType;
    public string GetID() => nodeID;
    public List<string> GetConnectedIDs() => connectedIDs;
    public Vector3 GetPosition() => position;
}
