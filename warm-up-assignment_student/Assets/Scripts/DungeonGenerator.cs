using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using NaughtyAttributes;
using UnityEngine.Analytics;
using Unity.AI.Navigation;
using UnityEngine.UIElements;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    public List<RectInt> rooms = new List<RectInt>();
    public List<RectInt> walls = new List<RectInt>();
    public List<RectInt> doors = new List<RectInt>();
    public int maxSplits = 3; // number of splits that should happen
    public int overlapSize = 2; // Total overlap (1 on each side)
    public int minRoomSize = 20; // Min width or height for a room to be able to split

    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public Transform dungeonParent;
    public NavMeshSurface navMeshSurface;

    [SerializeField]
    private DungeonGraphHelper dungeonGraphHelper;

    float duration = 0;
    bool depthTest = false;
    float height = 0.01f;

    [SerializeField]
    bool EnableTimers = false;

    private void Start()
    {
        RectInt initialRoom = new RectInt(0, 0, 100, 100);
        rooms.Add(initialRoom);

        // Create border wall
        RectInt borderRect = new RectInt(
            initialRoom.xMin - 1,
            initialRoom.yMin - 1,
            initialRoom.width + 2,
            initialRoom.height + 2
        );
        
        //top
        walls.Add(new RectInt(borderRect.xMin, borderRect.yMax - 1, borderRect.width, 1));
        //bottom
        walls.Add(new RectInt(borderRect.xMin, borderRect.yMin, borderRect.width, 1));
        //left
        walls.Add(new RectInt(borderRect.xMin, borderRect.yMin + 1, 1, borderRect.height - 2));
        //right
        walls.Add(new RectInt(borderRect.xMax - 1, borderRect.yMin + 1, 1, borderRect.height - 2));
        
        StartCoroutine(SplitOneRoom(initialRoom)); // making sure boundary remains unchanged
        
    }

    private void Update()
    {
        foreach (var room in rooms)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green, duration, depthTest, height);
        }
        foreach (var wall in walls)
        {
            AlgorithmsUtils.DebugRectInt(wall, Color.red, duration, depthTest, height);
        }
        foreach (var door in doors)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.yellow, duration, depthTest, height);
        }
    }

    IEnumerator SplitOneRoom(RectInt boundary)
    {
        for (int i = 0; i < maxSplits; i++)
        {
            yield return new WaitForEndOfFrame();
            if (rooms.Count == 0) continue; // Prevent errors

            // pick a random room from the list to split
            int roomIndex = Random.Range(0, rooms.Count);
            RectInt roomToSplit = rooms[roomIndex];

            bool splitVertically = Random.value > 0.5f;
            RectInt firstHalf, secondHalf;

            if (splitVertically)
            {
                int splitWidth = (roomToSplit.width / 2) + Random.Range(-5, 5);
                int splitX = roomToSplit.xMin + splitWidth;

                if ((roomToSplit.width / 2) < minRoomSize) continue;
                if (roomToSplit.height < minRoomSize) continue;

                firstHalf = new RectInt(roomToSplit.xMin, roomToSplit.yMin, splitWidth + 1, roomToSplit.height);
                secondHalf = new RectInt(splitX - 1, roomToSplit.yMin, roomToSplit.width - splitWidth + 1, roomToSplit.height);
            }
            else
            {
                int splitHeight = (roomToSplit.height / 2) + Random.Range(-5, 5);
                int splitY = roomToSplit.yMin + splitHeight;

                if ((roomToSplit.height / 2) < minRoomSize) continue;
                if (roomToSplit.width < minRoomSize) continue;

                firstHalf = new RectInt(roomToSplit.xMin, roomToSplit.yMin, roomToSplit.width, splitHeight + 1);
                secondHalf = new RectInt(roomToSplit.xMin, splitY - 1, roomToSplit.width, roomToSplit.height - splitHeight + 1);
            }

            // Ensure overlap is 1 on each side
            firstHalf.width = Mathf.Max(1, firstHalf.width);
            secondHalf.width = Mathf.Max(1, secondHalf.width);
            firstHalf.height = Mathf.Max(1, firstHalf.height);
            secondHalf.height = Mathf.Max(1, secondHalf.height);

            // replace the original room with the two new ones
            rooms.RemoveAt(roomIndex);
            rooms.Add(firstHalf);
            rooms.Add(secondHalf);
        }
        StartCoroutine(CalculateWalls());
    }



    IEnumerator CalculateWalls()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < rooms.Count; i++)
        {
            RectInt CheckedRoom = rooms[i];
            for (int j = i + 1; j < rooms.Count; j++)
            {
                RectInt CheckingRoom = rooms[j];
                RectInt intersection = AlgorithmsUtils.Intersect(CheckedRoom, CheckingRoom);
                if ((!(intersection.width == 2 && intersection.height == 2) && (intersection.width > 0 && intersection.height > 0)) && (intersection.width > 4 || intersection.height > 4))
                {
                    walls.Add(intersection);
                    
                }
            }
            
        }
        StartCoroutine(CalculateDoors());
    }
    

    IEnumerator CalculateDoors()
    {
        Debug.Log("Starting generation of walls and doors...");
        yield return new WaitForSeconds(1f);

        List<RectInt> newWalls = new List<RectInt>();

        foreach (RectInt wall in walls)
        {
            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Make sure wall is at least 6x2 or 2x6 to place a door
            if ((wall.width >= 6 && wall.height == 2) || (wall.height >= 6 && wall.width == 2))
            {
                bool isVertical = wall.width == 2;
                int minDoorPos = 2; // minimum distance from edge
                int maxDoorPos = (isVertical ? wall.height : wall.width) - 4; // max position for door

                if (maxDoorPos < minDoorPos) 
                {
                    newWalls.Add(wall); // If no space for a door, keep the original wall
                    continue;
                }

                // make sure 2x6 and 6x2 are still getting doors (6-4 = 2)
                int doorOffset = (maxDoorPos == 2) ? 2 : Random.Range(minDoorPos, maxDoorPos);

                RectInt door;
                RectInt wall1, wall2;

                if (isVertical)
                {
                    door = new RectInt(wall.x, wall.y + doorOffset, 2, 2);
                    wall1 = new RectInt(wall.x, wall.y, 2, doorOffset);
                    wall2 = new RectInt(wall.x, wall.y + doorOffset + 2, 2, wall.height - (doorOffset + 2));
                }
                else // horizontal wall
                {
                    door = new RectInt(wall.x + doorOffset, wall.y, 2, 2);
                    wall1 = new RectInt(wall.x, wall.y, doorOffset, 2);
                    wall2 = new RectInt(wall.x + doorOffset + 2, wall.y, wall.width - (doorOffset + 2), 2);
                }

                doors.Add(door);

                // add new walls
                if (wall1.width > 0 && wall1.height > 0) newWalls.Add(wall1);
                if (wall2.width > 0 && wall2.height > 0) newWalls.Add(wall2);
            }
            else
            {
                // keep the walls that are too small for doors
                newWalls.Add(wall);
            }
        }
        walls = newWalls;

        Debug.Log("Wall generation complete.");

        StartCoroutine(SpawnFloor());
    }

    IEnumerator SpawnFloor()
    {
        Debug.Log("Starting generation...");
        yield return new WaitForEndOfFrame();

        HashSet<Vector3> floorPositions = new HashSet<Vector3>(); // HashSet for floor positions

        foreach (RectInt room in rooms)
        {
            for (int x = room.xMin; x < room.xMax; x++)
            {
                for (int y = room.yMin; y < room.yMax; y++)
                {
                    Vector3 position = new Vector3(x + 0.5f, -0.5f, y + 0.5f); // Center each cube
                    floorPositions.Add(position);
                }
            }
        }

        foreach (Vector3 position in floorPositions)
        {
            Instantiate(floorPrefab, position, Quaternion.identity, dungeonParent);
            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.0001f); // Optional delay for debugging
            }
        }

        Debug.Log("I'm done generating floors hehehaha");
        StartCoroutine(SpawnWalls());
    }


    IEnumerator SpawnWalls()
    {
        Debug.Log("Spawning walls");
        yield return new WaitForEndOfFrame();

        HashSet<Vector3> wallPositions = new HashSet<Vector3>(); // HashSet for wall positions

        foreach (RectInt wall in walls)
        {
            for (int x = wall.xMin; x < wall.xMax; x++)
            {
                for (int y = wall.yMin; y < wall.yMax; y++)
                {
                    Vector3 position = new Vector3(x + 0.5f, 0.5f, y + 0.5f); // Center each cube
                    wallPositions.Add(position);
                }
            }
        }

        foreach (Vector3 position in wallPositions)
        {
            Instantiate(wallPrefab, position, Quaternion.identity, dungeonParent);
            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.01f); // Optional delay for debugging
            }
        }
    
        Debug.Log("I'm done spawning walls hehehaha");

    }

    [Button]
    void StartGraph()
    {
        dungeonGraphHelper.GenerateGraph(rooms, doors);
    }

    [Button]
    void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

}
