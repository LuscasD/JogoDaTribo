using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
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

    MapManager mapManager;
    Button button;
    Image nodeIcon;
    MapNode currentMapNode;
    [SerializeField] protected MapNodeTypes nodeType;
    [SerializeField] protected string nodeID;
    [SerializeField] protected List<string> connectedIDs = new List<string>();


    void Awake()
    {
        mapManager = MapManager.Instance;
        button = GetComponent<Button>();
        nodeIcon = GetComponent<Image>();

        button.onClick.AddListener(OnClick);
        switch (nodeType)
        {
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
        }
    }
    
    void Start()
    {
        currentMapNode = mapManager.GetCurrentMapNode();

        if (mapManager.GetClearedNodes().Contains(nodeID) || !currentMapNode.connectedIDs.Contains(nodeID))
        {
            button.interactable = false;
        }
    }

    void OnClick()
    {
        mapManager.AddClearedNode(nodeID);
        mapManager.SetCurrentMapNodeID(nodeID);
        mapManager.GoToScene("TestScene");
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



    public string GetID()
    {
        return nodeID;
    }

    public List<string> GetConnectedIDs()
    {
        return connectedIDs;
    }
}



