using System.Collections.Generic;
using Unity.VisualScripting;
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
    private MapNode currentMapNode;

    void Awake()
    {
        mapManager = MapManager.Instance;
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        nodeIcon = GetComponent<Image>();

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
            if (alreadyCleared)
            {
                nodeIcon.sprite = Resources.Load<Sprite>("ClearedIcon");
                nodeIcon.color = new Color(255,255,255,128); 
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
