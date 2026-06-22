using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{   
    static MapNode baseMapNode = Resources.Load<MapNode>("MapNode");
    static Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();


    public static List<List<MapNode>> GenerateBaseGrid(int height = 15, int width = 7)
    {
        List<List<MapNode>> grid = new List<List<MapNode>>();
        List<MapNode> row = new List<MapNode>();

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                row.Add(null);
            }
            grid.Add(new List<MapNode>(row));
        }
        
        return grid;
    }

    public static List<List<(int,int)>> GeneratePaths(int nOfPaths = 5, int height = 15, int width = 7)
    {
        List<List<(int,int)>> generatedPaths = new List<List<(int,int)>>();
        List<(int,int)> path = new List<(int, int)>();
        List<(int,int)> generatedPath = new List<(int, int)>();

        for (int j = 0; j < nOfPaths; j++)
        {
            int pastPos = 0;
            int xPos;
            bool crossed;

            for (int i = 0; i < height; i++)
            {
                
                do
                {
                    crossed = false;
                    if (i == 0)
                    {
                        xPos = Random.Range(0, width);
                    } 
                    else
                    {
                        xPos = Random.Range(pastPos - 1, pastPos + 2);
                    }

                    if (j > 0 && i > 0)
                    {
                        foreach (var gPath in generatedPaths)
                        {
                            (int topY, int topX) = gPath[i];
                            (int bottomY, int bottomX) = gPath[i - 1];
                            (int pastY, int pastX) = path[i - 1];

                            if(bottomX == xPos && bottomY == i - 1) //se o novo ponto gerado fica em cima do ponto de outro path
                            {
                                if(topX == pastX && topY == pastY + 1) //se o ponto anterior fica embaixo do proximo ponto do outro path
                                {
                                    crossed = true;
                                    Debug.Log("crossed");
                                }
                            }
                        }
                    }
                }
                while (xPos >= width || xPos < 0 || crossed);
                path.Add((i, xPos));
                pastPos = xPos;
            }
            string combinedString = string.Join(",", path);
            Debug.Log(combinedString);
            generatedPath.AddRange(path);

            generatedPaths.Add(path.ConvertAll(tuple => CloneTuple(tuple))); 
            
            path.Clear();
        } 
        return generatedPaths;
    }

    private static (int, int) CloneTuple((int, int) tuple)
    {
        (int a, int b) = tuple;
        return (a, b);
    }

    public static List<List<MapNode>> GenerateMapNodes(List<List<(int,int)>> paths, List<List<MapNode>> grid)
    {
        GameObject panel = GameObject.Find("Panel");
        string lastMapNodeName;

        MapNode start = Instantiate(baseMapNode);
        start.SetMapNodeType(MapNode.MapNodeTypes.Start);
        start.SetID("Start");
        start.transform.SetParent(panel.transform);
        start.transform.localPosition = new Vector2(0, -375);

        MapNode boss = Instantiate(baseMapNode);
        boss.SetMapNodeType(MapNode.MapNodeTypes.Boss);
        boss.SetID("Boss");
        boss.transform.SetParent(panel.transform);
        boss.transform.localPosition = new Vector2(0, -375);


        foreach (var path in paths)
        {
            lastMapNodeName = null;
            foreach (var node in path)
            {
                (int y, int x) = node;
                
                Debug.Log(y + ", " + x + ", " + grid[y][x]);
                if(grid[y][x] == null)
                {
                    grid[y][x] = GenerateMapNode(grid);
                    grid[y][x].transform.SetParent(panel.transform);
                    Debug.Log("thsi works actually");
                    
                }
                if(lastMapNodeName != null)
                {
                    grid[y][x].AddConnectedID(lastMapNodeName);
                }
                else
                {
                    grid[y][x].AddConnectedID(boss.GetID());
                }
                lastMapNodeName = grid[y][x].GetID();
            }
        }
        foreach (var node in grid[^1].FindAll(
            delegate(MapNode node)
            {
                return node != null;
            }
        ))
        {
            start.AddConnectedID(node.GetID());
        }

        List<MapNode> tips = new List<MapNode>
        {
            start,
            boss
        };
        grid.Add(tips);
        return grid;
    }

    public static void PositionMapNodes(List<List<(int,int)>> paths, List<List<MapNode>> grid)
    {
        Vector2 centerPos = canvas.transform.position;
        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width; //1920  960x540
        float basePos = -275;
        float ySize = 1500;
        float yOffset = 100;
        float xSize = 700;
        float xOffset = 150;

        float xPos;
        float yPos;

        foreach (var path in paths)
        {
            foreach (var node in path)
            {
                (int y, int x) = node;

                if(grid[y][x] != null)
                {
                    xPos = (xOffset*x)-(xSize/2);
                    yPos = ySize-(yOffset*y)+basePos;
                    grid[y][x].transform.localPosition = new Vector2(xPos, yPos);
                }
            }
        }
        foreach (var node in grid[^1])
        {
            if(node.GetID() == "Start")
            {
                node.transform.localPosition = new Vector2(0, basePos - 100);
            }
            if(node.GetID() == "Boss")
            {
                node.transform.localPosition = new Vector2(0, ySize + 100);
            }
        }
    }

    private static MapNode GenerateMapNode(List<List<MapNode>> rows)
    {
        MapNode mapNode = Instantiate(baseMapNode);
        int roll = Random.Range(1,100);
        if(roll <= 20)
        {
            mapNode.SetMapNodeType(MapNode.MapNodeTypes.Hub);
        } 
        else
        {
            roll = Random.Range(1,100);
            if(roll <= 25)
            {
                mapNode.SetMapNodeType(MapNode.MapNodeTypes.Battle);
            } 
            else if(roll <= 65)
            {
                mapNode.SetMapNodeType(MapNode.MapNodeTypes.ScrapBattle);
            }
            else
            {
                mapNode.SetMapNodeType(MapNode.MapNodeTypes.PartBattle);
            }
        }
        NameMapNode(mapNode, rows);
        
        return mapNode;
    }

    private static void NameMapNode(MapNode mapNode, List<List<MapNode>> rows)
    {
        string name = "";
        switch (mapNode.GetNodeType())
        {
            case MapNode.MapNodeTypes.Hub:
                name = "Hub";
                break;
            case MapNode.MapNodeTypes.Battle:
                name = "Battle";
                break;
            case MapNode.MapNodeTypes.PartBattle:
                name = "PartBattle";
                break;
            case MapNode.MapNodeTypes.ScrapBattle:
                name = "ScrapBattle";
                break;
        }

        int i = 0;
        foreach (var row in rows)
        {   
            List<MapNode> list = row.FindAll(
                delegate(MapNode node)
                {
                    return node != null;
                }
            );
            foreach (var node in list)
            {
                if(node.GetID().StartsWith(name))
                {
                    i++;
                }
            }

        }

        i++;
        mapNode.SetID(name + i);
    }
}
