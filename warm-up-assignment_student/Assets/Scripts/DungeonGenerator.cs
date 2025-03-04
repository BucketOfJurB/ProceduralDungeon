using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;

public class DungeonGenerator : MonoBehaviour
{
    public List<RectInt> rooms = new List<RectInt>(); // Store all rooms here
    public int maxSplits = 3; // Number of splits that should happen
    public int overlapSize = 2; // Total overlap (1 on each side)
    public int minRoomSize = 10; // Min width or height for a room to be able to split

    public GameObject floorPrefab;
    public Transform dungeonParent;

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
        StartCoroutine("DungeonGeneration");
        
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

    IEnumerator DungeonGeneration()
    {
        Debug.Log("Starting generation...");
        yield return new WaitForSeconds(1f);
        foreach (RectInt room in rooms)
        {
            // Calculate the center of the room for positioning
            Vector3 position = new Vector3(room.x + room.width / 2f, 0, room.y + room.height / 2f);

            // Create a new floor
            GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, dungeonParent);

            yield return new WaitForSeconds(0.5f);

            // Scale the floor to fit the room size
            floor.transform.localScale = new Vector3(room.width, 1, room.height);

            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("I'm done generating floors hehehaha");
        StartCoroutine(GenerateWallsAndDoors());
    }
    IEnumerator GenerateWallsAndDoors()
    {
        yield return null;
    }

}
//int splitX = Random.Range(currentRoom.xMin + 10, currentRoom.xMax - 10);
//int splitY = Random.Range(currentRoom.yMin + 10, currentRoom.yMax - 10);
