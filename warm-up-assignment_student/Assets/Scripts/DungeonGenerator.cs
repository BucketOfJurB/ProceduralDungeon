using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public List<RectInt> rooms = new List<RectInt>(); // Store all rooms here
    public int maxSplits = 2; // Number of times to divide the dungeon

    float duration = 0;
    bool depthTest = false;
    float height = 0.01f;

    private void Start()
    {
        RectInt initialRoom = new RectInt(0, 0, 100, 50);
        GenerateSubRooms(initialRoom, maxSplits);
    }

    private void Update()
    {
        // Draw all rooms
        foreach (var room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green, duration, depthTest, height);
        }
    }

    void GenerateSubRooms(RectInt currentRoom, int splitsRemaining)
    {
        if (splitsRemaining <= 0)
        {
            rooms.Add(currentRoom);
            return;
        }

        bool splitVertically = Random.value > 0.5f;
        if (splitVertically)
        {
            //int splitX = Random.Range(currentRoom.xMin + 5, currentRoom.xMax - 5);
            int halfWidth = currentRoom.width / 2; // Get half of the current width
            int splitX = currentRoom.xMin + halfWidth;

            RectInt leftRoom = new RectInt(currentRoom.xMin, currentRoom.yMin, splitX - currentRoom.xMin, currentRoom.height);
            RectInt rightRoom = new RectInt(splitX, currentRoom.yMin, currentRoom.xMax - splitX, currentRoom.height);

            //GenerateSubRooms(leftRoom, splitsRemaining - 1);
            //GenerateSubRooms(rightRoom, splitsRemaining - 1);
        }
        else
        {
            //int splitY = Random.Range(currentRoom.yMin + 5, currentRoom.yMax - 5);
            int halfHeight = currentRoom.height / 2; // Get half of the current height
            int splitY = currentRoom.yMin + halfHeight;

            RectInt bottomRoom = new RectInt(currentRoom.xMin, currentRoom.yMin, currentRoom.width, splitY - currentRoom.yMin);
            RectInt topRoom = new RectInt(currentRoom.xMin, splitY, currentRoom.width, currentRoom.yMax - splitY);

            //GenerateSubRooms(bottomRoom, splitsRemaining - 1);
            //GenerateSubRooms(topRoom, splitsRemaining - 1);
        }
    }
}
