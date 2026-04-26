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
        Start,
    }

    [SerializeField] protected MapNodeTypes nodeType;
    [SerializeField] protected string nodeID;
    [SerializeField] protected string sceneToLoad = "ScenaDeTeste";
    [SerializeField] protected List<string> connectedIDs = new List<string>();

    private MapManager mapManager;
    private Button button;
    private Image nodeIcon;
    private LineRenderer lineRenderer;
    private MapNode currentMapNode;
    private List<MapNode> connectedNodes;

    void Awake()
    {
        mapManager = MapManager.Instance;
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        nodeIcon = GetComponent<Image>();
        lineRenderer = GetComponent<LineRenderer>();

        //seleção da sprite
        switch (nodeType)
        {
            case MapNodeTypes.Start:
                nodeIcon.sprite = Resources.Load<Sprite>("StartIcon");
                break;
            case MapNodeTypes.Hub:
                nodeIcon.sprite = Resources.Load<Sprite>("HubIcon");
                break;
            case MapNodeTypes.Battle:
                nodeIcon.sprite = Resources.Load<Sprite>("BattleIcon");
                break;
            case MapNodeTypes.PartBattle:
                nodeIcon.sprite = Resources.Load<Sprite>("PartIcon");
                break;
            case MapNodeTypes.ScrapBattle:
                nodeIcon.sprite = Resources.Load<Sprite>("ScrapIcon");
                break;
            case MapNodeTypes.Boss:
                nodeIcon.sprite = Resources.Load<Sprite>("BossIcon");
                break;
        }


        if (mapManager.GetCurrentMapNodeID() == nodeID)
        {
            mapManager.SetCurrentMapNode(this);
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
        } else
        {
            lineRenderer.startColor = Color.gray;
            lineRenderer.endColor = Color.gray;
        }
        
        

        mapManager.AddNodeToList(this);
    }

    void Start()
    {
        currentMapNode = mapManager.GetCurrentMapNode();

        if (currentMapNode == null)
        {
            button.interactable = false;
            return;
        }

        //desativar botões inacessíveis
        bool alreadyCleared = mapManager.GetClearedNodes().Contains(nodeID);
        bool isReachable    = currentMapNode.connectedIDs.Contains(nodeID);

        if(nodeType == MapNodeTypes.Start && !alreadyCleared)
            mapManager.AddClearedNode(nodeID);

        if (alreadyCleared || !isReachable)
            button.interactable = false;


        //criar linhas e trocar sprite de node terminado
        int index = 0;
        if (alreadyCleared)
        {
            nodeIcon.sprite = Resources.Load<Sprite>("ClearedIcon");
            nodeIcon.color = new Color(255,255,255,128);

            foreach (var ID in connectedIDs)
            {
                if (mapManager.GetClearedNodes().Contains(ID))
                {
                    MapNode node = mapManager.GetNodeFromID(ID);
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(index, node.transform.position);
                    lineRenderer.SetPosition(index + 1, transform.position);
                    lineRenderer.startColor = Color.yellow;
                    lineRenderer.endColor = Color.yellow;
                    return;
                }

            }
        }
        //criar resto das linhas
        connectedNodes = mapManager.GetConnectedNodes(this);
        lineRenderer.positionCount = connectedNodes.Count * 2;
        if (connectedNodes.Count > 0)
            foreach (var node in connectedNodes)
            {
                lineRenderer.SetPosition(index, node.transform.position);
                lineRenderer.SetPosition(index + 1, transform.position);
                index = index + 2;
            }

        
    }

    void OnClick()
    {
        mapManager.SetCurrentMapNodeID(nodeID);
        mapManager.GoToScene(sceneToLoad);
        /*switch (nodeType)
        {
            case MapNodeTypes.Hub:
                mapManager.GoToScene("Hub");
                break;
            case MapNodeTypes.Battle:
                mapManager.GoToScene("Battle");
                break;
            case MapNodeTypes.PartBattle:
                mapManager.GoToScene("PartBattle");
                break;
            case MapNodeTypes.ScrapBattle:
                mapManager.GoToScene("ScrapBattle");
                break;
            case MapNodeTypes.Boss:
                mapManager.GoToScene("Boss");
                break;
        }*/
    }

    public string GetID() => nodeID;
    public List<string> GetConnectedIDs() => connectedIDs;
}
