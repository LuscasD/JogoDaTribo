using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    private MapManager mapManager;
    private List<List<(int,int)>> path;
    private List<List<MapNode>> grid;

    void Awake()
    {
        mapManager = MapManager.Instance;
        //path = mapManager.GetPaths();
        //grid = mapManager.GetGrid();
    }

    void Start()
    {
        //mapManager.SpawnNodes(mapManager.GetSavedNodes());
        //MapGenerator.PositionMapNodes(path, grid);
    }
}
