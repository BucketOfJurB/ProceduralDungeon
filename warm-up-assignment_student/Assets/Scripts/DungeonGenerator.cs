using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DungeonGenerator : MonoBehaviour
{
    public List<RectInt> rooms = new List<RectInt>(); // Store all rooms here
    public int maxSplits = 3; // Number of splits that should happen
    public int overlapSize = 2; // Total overlap (1 on each side)
    public int minRoomSize = 10; // Min width or height for a room to be able to split

    float duration = 0;
    bool depthTest = false;
    float height = 0.01f;

    private void Start()
    {
        RectInt initialRoom = new RectInt(0, 0, 100, 100);
        rooms.Add(initialRoom);

        for (int i = 0; i < maxSplits; i++)
        {
            SplitOneRoom(initialRoom); // Ensure boundary remains unchanged
        }
    }

    private void Update()
    {
        foreach (var room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green, duration, depthTest, height);
        }
    }

    void SplitOneRoom(RectInt boundary)
    {
        if (rooms.Count == 0) return; // Prevent errors

        // Pick a random room from the list to split
        int roomIndex = Random.Range(0, rooms.Count);
        RectInt roomToSplit = rooms[roomIndex];

        bool splitVertically = Random.value > 0.5f;
        RectInt firstHalf, secondHalf;

        if (splitVertically)
        {
            int halfWidth = roomToSplit.width / 2;
            int splitX = roomToSplit.xMin + halfWidth;

            if (halfWidth < minRoomSize) return;

            firstHalf = new RectInt(roomToSplit.xMin, roomToSplit.yMin, halfWidth + 1, roomToSplit.height);
            secondHalf = new RectInt(splitX - 1, roomToSplit.yMin, roomToSplit.width - halfWidth + 1, roomToSplit.height);
        }
        else
        {
            int halfHeight = roomToSplit.height / 2;
            int splitY = roomToSplit.yMin + halfHeight;

            if (halfHeight < minRoomSize) return;

            firstHalf = new RectInt(roomToSplit.xMin, roomToSplit.yMin, roomToSplit.width, halfHeight + 1);
            secondHalf = new RectInt(roomToSplit.xMin, splitY - 1, roomToSplit.width, roomToSplit.height - halfHeight + 1);
        }

        // Ensure overlap is EXACTLY 2 units (1 on each side)
        firstHalf.width = Mathf.Max(1, firstHalf.width);
        secondHalf.width = Mathf.Max(1, secondHalf.width);
        firstHalf.height = Mathf.Max(1, firstHalf.height);
        secondHalf.height = Mathf.Max(1, secondHalf.height);

        // Replace the original room with the two new ones
        rooms.RemoveAt(roomIndex);
        rooms.Add(firstHalf);
        rooms.Add(secondHalf);
    }
}
//int splitX = Random.Range(currentRoom.xMin + 5, currentRoom.xMax - 5);
//int splitY = Random.Range(currentRoom.yMin + 5, currentRoom.yMax - 5);
