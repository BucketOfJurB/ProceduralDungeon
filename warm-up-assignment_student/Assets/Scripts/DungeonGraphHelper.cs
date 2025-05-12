using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DungeonGraphHelper : MonoBehaviour
{
    private DungeonGraph dungeonGraph;
    public void GenerateGraph(List<RectInt> rooms, List<RectInt> doors)
    {
        dungeonGraph = new DungeonGraph();
        dungeonGraph.BuildRoomGraph(rooms, doors);
    }

    void Update()
    {
        // Redraw the graph every frame
        if (dungeonGraph != null)
        {
            dungeonGraph.DrawGraph(0f);
        }
    }
}
