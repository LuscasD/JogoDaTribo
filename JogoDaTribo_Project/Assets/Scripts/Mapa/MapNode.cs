using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    protected enum MapNodeTypes
    {
        Battle,
        ScrapBattle,
        PartBattle,
        Hub,
        Boss,
    }

    [SerializeField] protected MapNodeTypes nodeType;
    [SerializeField] protected string nodeID;
    [SerializeField] protected string sceneToLoad = "ScenaDeTeste";
    [SerializeField] protected List<string> connectedIDs = new List<string>();

    private MapManager mapManager;
    private Button button;
    private MapNode currentMapNode;

    void Awake()
    {
        mapManager = MapManager.Instance;
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        if (mapManager.GetCurrentMapNodeID() == nodeID)
            mapManager.SetCurrentMapNode(this);
    }

    void Start()
    {
        currentMapNode = mapManager.GetCurrentMapNode();

        if (currentMapNode == null)
        {
            button.interactable = false;
            return;
        }

        bool alreadyCleared = mapManager.GetClearedNodes().Contains(nodeID);
        bool isReachable    = currentMapNode.connectedIDs.Contains(nodeID);

        if (alreadyCleared || !isReachable)
            button.interactable = false;
    }

    void OnClick()
    {
        mapManager.SetCurrentMapNodeID(nodeID);
        mapManager.GoToScene(sceneToLoad);
    }

    public string GetID() => nodeID;
    public List<string> GetConnectedIDs() => connectedIDs;
}
