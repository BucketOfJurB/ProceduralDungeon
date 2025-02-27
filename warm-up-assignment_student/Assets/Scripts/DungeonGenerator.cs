using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public List<RectInt> rooms = new List<RectInt>(); // Store all rooms here
    public int maxSplits = 2; // Number of times to divide the dungeon

    float duration = 100;
    bool depthTest = false;
    float height = 0.01f;

    private void Start()
    {
        RectInt initialRoom = new RectInt(0, 0, 100, 50);
        rooms.Add(initialRoom);

        for (int i = 0; i < maxSplits; i++)
        {
            SplitOneRoom();
        }
    }

    private void Update()
    {
        // Draw all rooms
        foreach (var room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green, duration, depthTest, height);
        }
    }

    void SplitOneRoom()
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

            firstHalf = new RectInt(
                roomToSplit.xMin, 
                roomToSplit.yMin, 
                halfWidth, 
                roomToSplit.height 
                );
            secondHalf = new RectInt(
                splitX, 
                roomToSplit.yMin, 
                roomToSplit.width - halfWidth, 
                roomToSplit.height
                );
        }
        else
        {
            int halfHeight = roomToSplit.height / 2;
            int splitY = roomToSplit.yMin + halfHeight;

            firstHalf = new RectInt(
                roomToSplit.xMin,
                roomToSplit.yMin, 
                roomToSplit.width, 
                halfHeight
                );

            secondHalf = new RectInt(
                roomToSplit.xMin, 
                splitY, 
                roomToSplit.width, 
                roomToSplit.height - halfHeight
                );
        }

        // Replace the original room with the two new ones
        rooms.RemoveAt(roomIndex);
        rooms.Add(firstHalf);
        rooms.Add(secondHalf);
    }
}
            //int splitX = Random.Range(currentRoom.xMin + 5, currentRoom.xMax - 5);
            //int splitY = Random.Range(currentRoom.yMin + 5, currentRoom.yMax - 5);
