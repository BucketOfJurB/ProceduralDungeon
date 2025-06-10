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
        return false;
        //check if 1 room can connect to all other rooms, if room+connected rooms = total rooms(.count) then true else false.
    }

}
