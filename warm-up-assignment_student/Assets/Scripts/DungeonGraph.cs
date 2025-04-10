using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static AlgorithmsUtils;


public class DungeonGraph
{
    public Dictionary<RectInt, List<RectInt>> GenerateGraph(List<RectInt> rooms, List<RectInt> doors)
    {
        var graph = new Dictionary<RectInt, List<RectInt>>();
        foreach (var room in rooms)
        {
            graph[room] = new List<RectInt>();
        }

        foreach (var door in doors)
        {
            // find rooms that intersect with a door
            var connectedRooms = rooms.Where(r => AlgorithmsUtils.Intersects(r, door)).ToList();
            
            // Expect exactly two rooms per door
            if (connectedRooms.Count == 2)
            {
                // Add edges between the two rooms
                graph[connectedRooms[0]].Add(connectedRooms[1]);
                graph[connectedRooms[1]].Add(connectedRooms[0]);
            }
            else
            {
                // Warn if the door doesn’t connect exactly two rooms
                Debug.LogWarning($"Door at {door.position} connects {connectedRooms.Count} rooms, expected 2.");
            }
        }

        return graph;
    }
}