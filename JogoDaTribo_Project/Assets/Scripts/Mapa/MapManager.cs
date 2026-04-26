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


    private void Awake()
    {
        clearedNodes = new List<string>();
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
	

	public void GoToScene(string sceneName)
	{
		SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
	}
	//funções de troca de cena
	//fade in fade out?? overlay no canvas, dontdestroyonload
}