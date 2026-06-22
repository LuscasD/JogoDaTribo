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
	private List<string> clearedNodes = new List<string>();
	private List<MapNode> nodeList = new List<MapNode>();

	static MapNode baseMapNode = Resources.Load<MapNode>("MapNode");

	private List<List<(int,int)>> path;
	private List<List<MapNode>> grid;
	private List<MapNodeSave> savedNodes = new List<MapNodeSave>();

    private void Awake()
    {
		currentMapNodeID = "Start";
		path = MapGenerator.GeneratePaths();
		grid = MapGenerator.GenerateBaseGrid();
		MapGenerator.GenerateMapNodes(path, grid);
		MapGenerator.PositionMapNodes(path, grid);
		SaveNodes(grid);
    }

    private void Start()
    {
		Debug.Log(grid);
		SceneManager.sceneLoaded += OnSceneLoaded;
    }

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(scene.name == "WorldMap")
		{
			SpawnNodes();
		}
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

	public bool IsReachable(string ID)
	{
		return currentMapNode.GetConnectedIDs().Contains(ID);
	}

	public List<string> GetClearedNodes()
	{
		return clearedNodes;
	}

	public void AddClearedNode(string nodeID)
	{
		clearedNodes.Add(nodeID);
	}
	
	public List<List<(int,int)>> GetPaths()
	{
		return path;
	}

	public List<List<MapNode>> GetGrid()
	{
		return grid;
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


 	private void SaveNodes(List<List<MapNode>> grid)
	{
		List<MapNode> listToSave = new List<MapNode>();
		foreach (var row in grid)
		{
			listToSave.AddRange(row.FindAll(
				delegate(MapNode node)
                {
                    return node != null;
                }
			));
		}
		foreach (var node in listToSave)
		{
			MapNodeSave savedNode = new MapNodeSave();
			savedNode.SaveNode(node);
			savedNodes.Add(savedNode);
		}
	}

	public void SpawnNodes()
	{
		MapNode baseMapNode = Resources.Load<MapNode>("MapNode");
		GameObject panel = GameObject.Find("Panel");
		foreach (var savedNode in savedNodes)
		{
			MapNode node = Instantiate(baseMapNode);
			node.transform.SetParent(panel.transform);
			node.SetMapNodeType(savedNode.GetNodeType());
			node.SetID(savedNode.GetID());
			node.SetConnectedIDs(savedNode.GetConnectedIDs());
			node.transform.localPosition = savedNode.GetPosition();
		}
	}

	public List<MapNodeSave> GetSavedNodes()
	{
		return savedNodes;
	}


	public void GoToScene(string sceneName)
	{
		nodeList.Clear();
		SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
	}
	//funções de troca de cena
	//fade in fade out?? overlay no canvas, dontdestroyonload
}