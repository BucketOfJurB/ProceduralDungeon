using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using NaughtyAttributes;
using UnityEngine.Analytics;
using Unity.AI.Navigation;
using UnityEngine.UIElements;

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

    float duration = 0;
    bool depthTest = false;
    float height = 0.01f;

    [SerializeField]
    bool EnableTimers = false;

    private void Start()
    {
        RectInt initialRoom = new RectInt(0, 0, 100, 100);
        rooms.Add(initialRoom);

        
        StartCoroutine(SplitOneRoom(initialRoom)); // Ensure boundary remains unchanged
        
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
        StartCoroutine(DungeonGeneration());
        //make sure you're not checking corners or rooms that have already been checked, also make sure to not check the same room on itself
    }


    IEnumerator DungeonGeneration()
    {
        Debug.Log("Starting generation...");
        yield return new WaitForEndOfFrame();
        foreach (RectInt room in rooms)
        {
            //calculate the center of the room for positioning
            Vector3 position = new Vector3(room.x + room.width / 2f, -0.5f, room.y + room.height / 2f);


            GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, dungeonParent);

            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // scale the floor to fit the room size
            floor.transform.localScale = new Vector3(room.width, 1, room.height);

            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        Debug.Log("I'm done generating floors hehehaha");
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

        StartCoroutine(SpawnWalls());
    }

    
    IEnumerator SpawnWalls()
    {
        Debug.Log("Spawning walls");
        yield return new WaitForEndOfFrame();
        foreach (RectInt wall in walls)
        {
            //calculate the center of the room for positioning
            Vector3 position = new Vector3(wall.x + wall.width / 2f, 0.5f, wall.y + wall.height / 2f);


            GameObject wallHalf = Instantiate(wallPrefab, position, Quaternion.identity, dungeonParent);

            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // scale the floor to fit the room size
            wallHalf.transform.localScale = new Vector3(wall.width, 1, wall.height);

            if (EnableTimers)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        Debug.Log("I'm done spawning walls hehehaha");


    }

    void CreateGraph(){
        DungeonGraph dungeonGraph = new DungeonGraph();
        Dictionary<RectInt, List<RectInt>> graph = dungeonGraph.GenerateGraph(rooms, doors);
    }

    [Button]
    void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

}
