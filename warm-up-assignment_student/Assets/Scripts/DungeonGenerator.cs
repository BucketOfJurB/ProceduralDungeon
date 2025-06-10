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
    public List<RectInt> boundaryWalls = new List<RectInt>();
    public List<RectInt> doors = new List<RectInt>();
    public int minRoomSize = 10; // Min width or height for a room

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
        boundaryWalls.Add(new RectInt(borderRect.xMin, borderRect.yMax - 1, borderRect.width, 1));
        //bottom
        boundaryWalls.Add(new RectInt(borderRect.xMin, borderRect.yMin, borderRect.width, 1));
        //left
        boundaryWalls.Add(new RectInt(borderRect.xMin, borderRect.yMin + 1, 1, borderRect.height - 2));
        //right
        boundaryWalls.Add(new RectInt(borderRect.xMax - 1, borderRect.yMin + 1, 1, borderRect.height - 2));
        
        StartCoroutine(SplitUntilCannotSplit(initialRoom)); // making sure boundary remains unchanged
        
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

    IEnumerator SplitUntilCannotSplit(RectInt boundary)
    {
        bool didSplit = true;
        while (didSplit)
        {
            didSplit = false;
            for (int i = 0; i < rooms.Count; i++)
            {
                RectInt roomToSplit = rooms[i];

                bool canSplitVertically = roomToSplit.width >= minRoomSize * 2;
                bool canSplitHorizontally = roomToSplit.height >= minRoomSize * 2;

                if (!canSplitVertically && !canSplitHorizontally)
                    continue;

                bool splitVertically = canSplitVertically && (!canSplitHorizontally || Random.value > 0.5f);

                RectInt firstHalf, secondHalf;

                if (splitVertically)
                {
                    int minSplit = roomToSplit.xMin + minRoomSize;
                    int maxSplit = roomToSplit.xMax - minRoomSize;
                    if (minSplit >= maxSplit) continue;

                    int splitX = Random.Range(minSplit, maxSplit);

                    // overlap by 1 on the shared edge (So x=splitX-1 is shared)
                    firstHalf = new RectInt(roomToSplit.xMin, roomToSplit.yMin, splitX - roomToSplit.xMin, roomToSplit.height);
                    secondHalf = new RectInt(splitX - 1, roomToSplit.yMin, roomToSplit.xMax - (splitX - 1), roomToSplit.height);
                }
                else
                {
                    int minSplit = roomToSplit.yMin + minRoomSize;
                    int maxSplit = roomToSplit.yMax - minRoomSize;
                    if (minSplit >= maxSplit) continue;

                    int splitY = Random.Range(minSplit, maxSplit);

                    // overlap by 1 on the shared edge (so y=splitY-1 is shared)
                    firstHalf = new RectInt(roomToSplit.xMin, roomToSplit.yMin, roomToSplit.width, splitY - roomToSplit.yMin);
                    secondHalf = new RectInt(roomToSplit.xMin, splitY - 1, roomToSplit.width, roomToSplit.yMax - (splitY - 1));
                }

                // Final check for minRoomSize
                if (firstHalf.width < minRoomSize || firstHalf.height < minRoomSize ||
                    secondHalf.width < minRoomSize || secondHalf.height < minRoomSize)
                    continue;

                //  clamp to boundary
                firstHalf.xMin = Mathf.Max(boundary.xMin, firstHalf.xMin);
                firstHalf.yMin = Mathf.Max(boundary.yMin, firstHalf.yMin);
                firstHalf.xMax = Mathf.Min(boundary.xMax, firstHalf.xMax);
                firstHalf.yMax = Mathf.Min(boundary.yMax, firstHalf.yMax);

                secondHalf.xMin = Mathf.Max(boundary.xMin, secondHalf.xMin);
                secondHalf.yMin = Mathf.Max(boundary.yMin, secondHalf.yMin);
                secondHalf.xMax = Mathf.Min(boundary.xMax, secondHalf.xMax);
                secondHalf.yMax = Mathf.Min(boundary.yMax, secondHalf.yMax);

                // update width/height after clamping
                firstHalf.width = firstHalf.xMax - firstHalf.xMin;
                firstHalf.height = firstHalf.yMax - firstHalf.yMin;
                secondHalf.width = secondHalf.xMax - secondHalf.xMin;
                secondHalf.height = secondHalf.yMax - secondHalf.yMin;

                //replace original with two new rooms
                rooms.RemoveAt(i);
                rooms.Add(firstHalf);
                rooms.Add(secondHalf);
                didSplit = true;
                break;
            }
            yield return new WaitForEndOfFrame();
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
                if ((!(intersection.width == 1 && intersection.height == 1) && (intersection.width > 0 && intersection.height > 0)) && (intersection.width > 2 || intersection.height > 2))
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

            //now 1 thick walls
            if ((wall.width == 1 && wall.height >= 3) || (wall.height == 1 && wall.width >= 3))
            {
                bool isVertical = wall.width == 1;
                int minDoorPos = 1; // minimum distance from edge
                int maxDoorPos = (isVertical ? wall.height : wall.width) - 2; // max position for door (so at least 1 on each side)

                if (maxDoorPos < minDoorPos)
                {
                    newWalls.Add(wall); // If no space for a door, keep the original wall
                    continue;
                }

                int doorOffset = Random.Range(minDoorPos, maxDoorPos + 1); // +1 so maxDoorPos is included

                RectInt door;
                RectInt wall1, wall2;

                if (isVertical)
                {
                    // Vertical wall, door is 1x1
                    door = new RectInt(wall.x, wall.y + doorOffset, 1, 1);
                    wall1 = new RectInt(wall.x, wall.y, 1, doorOffset);
                    wall2 = new RectInt(wall.x, wall.y + doorOffset + 1, 1, wall.height - (doorOffset + 1));
                }
                else // horizontal wall
                {
                    door = new RectInt(wall.x + doorOffset, wall.y, 1, 1);
                    wall1 = new RectInt(wall.x, wall.y, doorOffset, 1);
                    wall2 = new RectInt(wall.x + doorOffset + 1, wall.y, wall.width - (doorOffset + 1), 1);
                }

                doors.Add(door);

                // prevents errors even though size 0 shouldn't be possible
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
                    Vector3 position = new Vector3(x + 0.5f, -0.5f, y + 0.5f); //center each cube
                    floorPositions.Add(position);
                }
            }
        }

        foreach (Vector3 position in floorPositions)
        {
            Instantiate(floorPrefab, position, Quaternion.identity, dungeonParent);
            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.001f);
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
                    Vector3 position = new Vector3(x + 0.5f, 0.5f, y + 0.5f); //center each cube
                    wallPositions.Add(position);
                }
            }
        }

        foreach (RectInt boundaryWall in boundaryWalls)
        {
            for (int x = boundaryWall.xMin; x < boundaryWall.xMax; x++)
            {
                for (int y = boundaryWall.yMin; y < boundaryWall.yMax; y++)
                {
                    Vector3 position = new Vector3(x + 0.5f, 0.5f, y + 0.5f); //center each cube again :))
                    wallPositions.Add(position);
                }
            }
        }

        foreach (Vector3 position in wallPositions)
        {
            Instantiate(wallPrefab, position, Quaternion.identity, dungeonParent);
            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.01f);
            }
        }
    
        Debug.Log("I'm done spawning walls hehehaha");

    }

   

    [Button]
    void StartGraph()
    {
        dungeonGraphHelper.GenerateGraph(rooms, doors);
        Debug.Log(dungeonGraphHelper.IsConnected());
    }

    [Button]
    void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

}
