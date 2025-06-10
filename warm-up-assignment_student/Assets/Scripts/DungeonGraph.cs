using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGraph
{
    public Graph<RectInt> RoomGraph { get; private set; }
    private Dictionary<(RectInt, RectInt), RectInt> edgeToDoor;
    private HashSet<(RectInt, RectInt)> drawnEdges;

    public DungeonGraph()
    {
        RoomGraph = new Graph<RectInt>();
        edgeToDoor = new Dictionary<(RectInt, RectInt), RectInt>();
        drawnEdges = new HashSet<(RectInt, RectInt)>();
    }

    public void BuildRoomGraph(List<RectInt> rooms, List<RectInt> doors)
    {
        if (rooms == null || doors == null)
        {
            Debug.LogError("Rooms or doors list is null.");
            return;
        }

        // Add all rooms as nodes
        foreach (var room in rooms)
        {
            RoomGraph.AddNode(room);
        }

        // Add edges based on doors
        foreach (var door in doors)
        {
            var connectedRooms = rooms.Where(r => AlgorithmsUtils.Intersects(r, door)).ToList();
            if (connectedRooms.Count == 2)
            {
                RectInt room1 = connectedRooms[0];
                RectInt room2 = connectedRooms[1];
                RoomGraph.AddEdge(room1, room2);
                edgeToDoor[(room1, room2)] = door;
                edgeToDoor[(room2, room1)] = door;
            }
            else
            {
                Debug.LogWarning($"Door at {door.position} connects {connectedRooms.Count} rooms, expected 2.");
            }
        }
    }

    public void DrawGraph(float duration = 0f)
    {
        if (RoomGraph == null)
        {
            Debug.LogError("RoomGraph is null. Call BuildRoomGraph first.");
            return;
        }

        foreach (var room in RoomGraph.GetNodes())
        {
            Vector3 roomCenter = GetRoomCenter(room);
            var neighbors = RoomGraph.GetNeighbors(room);
            foreach (var neighbor in neighbors)
            {
                var edgeKey = room.GetHashCode() < neighbor.GetHashCode() ? (room, neighbor) : (neighbor, room);
                if (!drawnEdges.Contains(edgeKey))
                {
                    drawnEdges.Add(edgeKey);
                    if (edgeToDoor.TryGetValue(edgeKey, out RectInt door))
                    {
                        Vector3 doorCenter = GetDoorCenter(door);
                        Vector3 neighborCenter = GetRoomCenter(neighbor);
                        Debug.DrawLine(roomCenter, doorCenter, Color.blue, duration);
                        Debug.DrawLine(doorCenter, neighborCenter, Color.blue, duration);
                    }
                    else
                    {
                        Vector3 neighborCenter = GetRoomCenter(neighbor);
                        Debug.DrawLine(roomCenter, neighborCenter, Color.red, duration);
                        Debug.LogWarning($"No door found for edge between rooms at {room.position} and {neighbor.position}.");
                    }
                }
            }
        }
        drawnEdges.Clear();
    }
    

    private Vector3 GetRoomCenter(RectInt room)
    {
        Vector2 center = new Vector2(room.x + room.width / 2f, room.y + room.height / 2f);
        return new Vector3(center.x, 1, center.y);
    }

    private Vector3 GetDoorCenter(RectInt door)
    {
        Vector2 center = new Vector2(door.x + door.width / 2f, door.y + door.height / 2f);
        return new Vector3(center.x, 1, center.y);
    }
}