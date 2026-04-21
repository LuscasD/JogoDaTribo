using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UI;
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
    MapNode currentMapNode;
    [SerializeField] protected MapNodeTypes nodeType;
    [SerializeField] protected string nodeID;
    [SerializeField] protected List<string> connectedIDs = new List<string>();

    void Awake()
    {
        mapManager = MapManager.Instance;
        button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);

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


    void Update()
    {
        
    }

    void OnClick()
    {
        transform.position += Vector3.right * 10;
    }
}
