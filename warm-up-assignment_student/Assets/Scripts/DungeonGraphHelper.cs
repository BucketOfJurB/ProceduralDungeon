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

    public bool IsConnected()
    {
        if (dungeonGraph == null || dungeonGraph.RoomGraph == null)
            return false;

        var allRooms = dungeonGraph.RoomGraph.GetNodes();
        if (allRooms.Count == 0)
            return true; // connected.... technically

        // start BFS from the first room
        var visited = dungeonGraph.RoomGraph.BFS(allRooms[0]);
        return visited.Count == allRooms.Count;
    }

}
