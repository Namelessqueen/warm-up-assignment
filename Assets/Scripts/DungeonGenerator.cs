using NaughtyAttributes;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt startRoom;
    public Vector2 minRoomSizeRage = new Vector2(10, 20);
    Graph<RectInt> graph = new Graph<RectInt>();

    Coroutine drawCoroutine;
    List<RectInt> roomsToDraw = new List<RectInt>();
    List<RectInt> roomsToSplit = new List<RectInt>();
    List<RectInt> Doors = new List<RectInt>();

    public bool skipCoroutine = false;


    private List<Vector2> Walls = new List<Vector2>();
    private List<GameObject> Wallparents = new List<GameObject>();

    [SerializeField]
    private GameObject FloorPrefab;
    [SerializeField]
    private GameObject WallPrefab;

    private GameObject WallsParent;
    private GameObject FloorParent;

    NavMeshSurface navMeshSurface;


    void Start()
    {
        roomsToSplit.Add(startRoom);
        drawCoroutine = StartCoroutine(DrawCoroutine());
    }

    void Update()
    {
        Drawing();
    }

    void Drawing()
    {
        //Rooms
        AlgorithmsUtils.DebugRectInt(startRoom, Color.red);

        for (int i = 0; i < roomsToSplit.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(roomsToSplit[i], Color.red);
        }

        for (int i = 0; i < roomsToDraw.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(roomsToDraw[i], Color.green);
        }

        //Doors
        for (int i = 0; i < Doors.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(Doors[i], Color.cyan);
        }

        //Nodes
        for(int i = 0;i < graph.GetNodeCount(); i++)
        {
            Vector3 doorpos = new Vector3(graph.GetNodes()[i].center.x, 0, graph.GetNodes()[i].center.y);
            DebugExtension.DebugWireSphere(doorpos, Color.blue, 2f);

            for (int j = 0; j < graph.GetNeighbors(graph.GetNodes()[i]).Count; j++)
            {
                Vector3 roomPos = new Vector3(graph.GetNeighbors(graph.GetNodes()[i])[j].center.x, 0, graph.GetNeighbors(graph.GetNodes()[i])[j].center.y); ;
                Debug.DrawLine(doorpos, roomPos,Color.yellow);
            } 
        }


    }

    [Button]
    public void CreateDungeon()
    {
        roomsToSplit.Clear(); roomsToDraw.Clear(); Doors.Clear();

        roomsToSplit.Add(startRoom);
        drawCoroutine = StartCoroutine(DrawCoroutine());
    }
    IEnumerator DrawCoroutine()
    {

        //Rooms
        yield return new WaitForEndOfFrame();
        Debug.Log("coroutine start");

        while (roomsToSplit.Count > 0)
        {
            RectInt currentRoom = roomsToSplit[0];
            roomsToSplit.Remove(currentRoom);

            int minRoomSizeRange = Random.Range((int)minRoomSizeRage.x, (int)minRoomSizeRage.y);

            if (currentRoom.width < minRoomSizeRange * 2 && currentRoom.height < minRoomSizeRange * 2)
            {
                roomsToDraw.Add(currentRoom);
            }
            else SplitRooms(currentRoom);

            if(!skipCoroutine)yield return new WaitForSeconds(0.1f);
        }

        for (int i = 0; i < roomsToDraw.Count; i++)
        {
            for (int j = i + 1; j < roomsToDraw.Count; j++)
            {
                if (AlgorithmsUtils.Intersects(roomsToDraw[i], roomsToDraw[j]))
                {
                    RectInt Inter = AlgorithmsUtils.Intersect(roomsToDraw[i], roomsToDraw[j]);
                    if ((Inter.width == 1 && Inter.height > 5) || (Inter.height == 1 && Inter.width > 5))
                    {
                        MakeDoor(roomsToDraw[i], roomsToDraw[j]);
                        if (!skipCoroutine) yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

        for (int i = 0; i < roomsToDraw.Count; i++)
        {
            for (int j = i + 1; j < roomsToDraw.Count; j++)
            {
                //graph.AddEdge(roomsToDraw[i], roomsToDraw[j]);
                //yield return new WaitForSeconds(0.1f);
            }
        }

        graph.BFS(graph.GetNodes()[0]) ;

        Debug.Log("drawing is done; Total room count: " + roomsToDraw.Count + "|  total Intersections: " + Doors.Count);

    }
    void SplitRooms(RectInt pRoom)
    {
        RectInt room1, room2; int splitRandom;

        if (pRoom.width >= pRoom.height)//vertical
        {
            splitRandom = Random.Range((int)minRoomSizeRage.x, pRoom.width - (int)minRoomSizeRage.x);

            room1 = new RectInt(pRoom.x, pRoom.y, splitRandom + 1, pRoom.height);
            room2 = new RectInt(pRoom.x + splitRandom, pRoom.y, pRoom.width - splitRandom, pRoom.height);
        }
        else//horizontal
        {
            splitRandom = Random.Range((int)minRoomSizeRage.x, pRoom.height - (int)minRoomSizeRage.x);

            room1 = new RectInt(pRoom.x, pRoom.y, pRoom.width, splitRandom + 1);
            room2 = new RectInt(pRoom.x, pRoom.y + splitRandom, pRoom.width, pRoom.height - splitRandom);

        }
        //Debug.Log(splitRandom);
        roomsToSplit.Insert(0, room1); roomsToSplit.Insert(1, room2);
    }

    void MakeDoor(RectInt room1, RectInt room2)
    {
        RectInt door = new RectInt(0, 0, 0, 0); int positionRandom;
        RectInt intersect = AlgorithmsUtils.Intersect(room1, room2);

        if (intersect.width == 1)
        {
            positionRandom = Random.Range(2, intersect.height-3);
            door = new RectInt(intersect.x,intersect.y+ positionRandom, 1, 2);
        }
        else if (intersect.height == 1)
        {
            positionRandom = Random.Range(2, intersect.width - 3);
            door = new RectInt(intersect.x+ positionRandom, intersect.y, 2, 1);
        }
        else Debug.LogError("Intersection not valid");

        Doors.Add(door);
        graph.AddEdge(door, room1); graph.AddEdge(door, room2);

    }

    /// <summary> Assests
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    [Button]
    public void SpawnDungeonAssets()
    {
        Destroy(WallsParent);
        Destroy(FloorParent);
        Wallparents.Clear(); Walls.Clear();

        WallsParent = new GameObject("WallParent");
        FloorParent = new GameObject("ParentFloor");

        for (int i = 0; i < Doors.Count; i++)
        {
            Walls.Add(Doors[i].position);
            if(Doors[i].height > 1) Walls.Add(new Vector2(Doors[i].x, Doors[i].y+1));
            else if (Doors[i].width > 1) Walls.Add(new Vector2(Doors[i].x+1, Doors[i].y));
        }
        for (int i = 0; i < roomsToDraw.Count; i++)
        {
            
            spawnroom(roomsToDraw[i]);
        }
        CreateFloor();
    }
    private void spawnroom(RectInt rectInt)
    {
        //Debug.Log(rectInt);

        GameObject parentGameObject = new GameObject("Room: " + rectInt.position);
        Wallparents.Add(parentGameObject);

        for (int i = 0; i < rectInt.height; i++)
        {
            Vector2 postition = new Vector2(rectInt.x, rectInt.y + i);

            if (!Walls.Contains(postition))
            {
                var newObject = Instantiate(WallPrefab, new Vector3(postition.x, 0, postition.y), Quaternion.identity, parentGameObject.transform);
                newObject.name = "Wall: " + postition;
                Walls.Add(postition);
            }

            postition = new Vector2(rectInt.x + rectInt.width - 1, rectInt.y + i);
            if (!Walls.Contains(postition))
            {
                var newObject = Instantiate(WallPrefab, new Vector3(postition.x, 0, postition.y), Quaternion.identity, parentGameObject.transform);
                newObject.name = "Wall: " + postition;
                Walls.Add(postition);
            }
        }
        for (int i = 0; i < rectInt.width; i++)
        {
            Vector2 postition = new Vector2(rectInt.x + i, rectInt.y);

            if (!Walls.Contains(postition))
            {

                var newObject = Instantiate(WallPrefab, new Vector3(postition.x, 0, postition.y), Quaternion.identity, parentGameObject.transform);
                newObject.name = "Wall: " + postition;
                Walls.Add(postition);
            }
            postition = new Vector2(rectInt.x + i, rectInt.y + rectInt.height - 1);
            if (!Walls.Contains(postition))
            {
                var newObject = Instantiate(WallPrefab, new Vector3(postition.x, 0, postition.y), Quaternion.identity, parentGameObject.transform);
                newObject.name = "Wall: " + postition;
                Walls.Add(postition);
            }
        }
        parentGameObject.transform.position = new Vector3(0.5f, 0.5f, 0.5f);
        parentGameObject.transform.parent = WallsParent.transform;
    }

    public void CreateFloor()
    {
        GameObject parentGameObject = new GameObject("ParentFloor");
        for (int i = 0; i < roomsToDraw.Count; i++)
        {
            for (int j = 1; j < roomsToDraw[i].width - 1; j++)
            {
                for (int k = 1; k < roomsToDraw[i].height - 1; k++)
                {
                    Instantiate(FloorPrefab, new Vector3(j + roomsToDraw[i].x, 0, k + roomsToDraw[i].y), FloorPrefab.transform.rotation, parentGameObject.transform);
                }
            }
        }
        for(int i = 0;i < Doors.Count; i++)
        {
            Instantiate(FloorPrefab, new Vector3(Doors[i].x, 0, Doors[i].y), FloorPrefab.transform.rotation, parentGameObject.transform);

            if(Doors[i].height > 1) Instantiate(FloorPrefab, new Vector3(Doors[i].x, 0, Doors[i].y + 1), FloorPrefab.transform.rotation, parentGameObject.transform);
            else if (Doors[i].width > 1) Instantiate(FloorPrefab, new Vector3(Doors[i].x+1, 0, Doors[i].y), FloorPrefab.transform.rotation, parentGameObject.transform);
        }
        parentGameObject.transform.parent = FloorParent.transform;
        parentGameObject.transform.position = new Vector3(0.5f, 0.5f, 0.5f);
    }

    [Button]
    public void NavMeshSurface()
    {
        navMeshSurface.BuildNavMesh();
    }

}