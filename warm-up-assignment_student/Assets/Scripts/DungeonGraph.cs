using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static AlgorithmsUtils;


public class DungeonGraph
{
    // Property to access the underlying graph
    public Graph<RectInt> RoomGraph { get; private set; }

    // Constructor to initialize the graph
    public DungeonGraph()
    {
        RoomGraph = new Graph<RectInt>();
    }

    // Method to build the graph from rooms and doors
    public void GenerateGraph(List<RectInt> rooms, List<RectInt> doors)
    {
        // Step 1: Add all rooms as nodes
        foreach (var room in rooms)
        {
            RoomGraph.AddNode(room);
        }

        // Step 2: Add edges based on doors
        foreach (var door in doors)
        {
            // Find rooms that intersect with this door
            var connectedRooms = rooms.Where(r => AlgorithmsUtils.Intersects(r, door)).ToList();

            // Check if the door connects exactly two rooms
            if (connectedRooms.Count == 2)
            {
                // Add an edge between the two rooms
                RoomGraph.AddEdge(connectedRooms[0], connectedRooms[1]);
            }
            else
            {
                // Log a warning if the door doesn’t connect exactly two rooms
                Debug.LogWarning($"Door at {door.position} connects {connectedRooms.Count} rooms, expected 2.");
            }
        }
    }
}