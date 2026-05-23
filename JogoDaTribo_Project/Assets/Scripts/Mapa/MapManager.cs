using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
	private static MapManager _Instance;
	public static MapManager Instance
	{
		get
		{
			if (!_Instance)
			{
				_Instance = new GameObject().AddComponent<MapManager>();
				_Instance.name = _Instance.GetType().ToString();
				DontDestroyOnLoad(_Instance.gameObject);
			}
			return _Instance;
		}
	}

	private MapNode currentMapNode;
	private string currentMapNodeID;
	private List<string> clearedNodes;
	private List<MapNode> nodeList;


    private void Awake()
    {
        clearedNodes = new List<string>();
		nodeList = new List<MapNode>();
		currentMapNodeID = "Start";
    }


	public MapNode GetCurrentMapNode()
	{
		return currentMapNode;
	}

	public void SetCurrentMapNode(MapNode mapNode)
	{
		currentMapNode = mapNode;
	}

	public string GetCurrentMapNodeID()
	{
		return currentMapNodeID;
	}

	public void SetCurrentMapNodeID(string ID)
	{
		currentMapNodeID = ID;
	}

	public List<string> GetClearedNodes()
	{
		return clearedNodes;
	}

	public void AddClearedNode(string nodeID)
	{
		clearedNodes.Add(nodeID);
	}
	

	public void AddNodeToList(MapNode node)
	{
		nodeList.Add(node);
	}

	public MapNode GetNodeFromID(string nodeID)
	{
		foreach (var node in nodeList)
		{
			if(node.GetID() == nodeID)
				return node;
		}
		return null;
	}

	public List<MapNode> GetConnectedNodes(MapNode mapNode)
	{
		List<string> list = mapNode.GetConnectedIDs();
		List<MapNode> connectedNodes = new List<MapNode>();
		foreach (var node in nodeList)
		{
			if (list.Contains(node.GetID()))
			{
				connectedNodes.Add(node);
			}
		}
		
		return connectedNodes;
	}


	public void GoToScene(string sceneName)
	{
		nodeList.Clear();
		SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
	}
	//funções de troca de cena
	//fade in fade out?? overlay no canvas, dontdestroyonload
}