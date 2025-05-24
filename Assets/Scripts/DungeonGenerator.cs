using NaughtyAttributes;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class DungeonGenerator : MonoBehaviour
{
    public int _seed;

    public RectInt _startRoom;
    public Vector2 _minRoomSizeRage = new Vector2(10, 20);
    Graph<RectInt> _graph = new Graph<RectInt>();

    Coroutine drawCoroutine;
    List<RectInt> _roomsToDraw = new List<RectInt>();
    List<RectInt> _roomsToSplit = new List<RectInt>();
    List<RectInt> _doors = new List<RectInt>();

    public bool SkipCoroutine = false;


    private List<Vector2> _walls = new List<Vector2>();
    private List<GameObject> _wallparents = new List<GameObject>();

    [SerializeField]
    private GameObject FloorPrefab;
    [SerializeField]
    private GameObject WallPrefab;

    public GameObject Player;

    private GameObject WallsParent;
    private GameObject FloorParent;

    NavMeshSurface _navMeshSurface;


    void Start()
    {
        _roomsToSplit.Add(_startRoom);
        drawCoroutine = StartCoroutine(DrawCoroutine());
        _navMeshSurface = FindAnyObjectByType<NavMeshSurface>();
    }

    void Update()
    {
        Drawing();
    }

    void Drawing()
    {
        //Rooms
        AlgorithmsUtils.DebugRectInt(_startRoom, Color.red);

        for (int i = 0; i < _roomsToSplit.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(_roomsToSplit[i], Color.red);
        }

        for (int i = 0; i < _roomsToDraw.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(_roomsToDraw[i], Color.green);
        }

        //Doors
        for (int i = 0; i < _doors.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(_doors[i], Color.cyan);
        }

        //Nodes
        for (int i = 0; i < _graph.GetNodeCount(); i++)
        {
            Vector3 nodePos = new Vector3(_graph.GetNodes()[i].center.x, 0, _graph.GetNodes()[i].center.y);
            DebugExtension.DebugWireSphere(nodePos, Color.blue, 1.5f);

            for (int j = 0; j < _graph.GetNeighbors(_graph.GetNodes()[i]).Count; j++)
            {
                Vector3 roomPos = new Vector3(_graph.GetNeighbors(_graph.GetNodes()[i])[j].center.x, 0, _graph.GetNeighbors(_graph.GetNodes()[i])[j].center.y); ;
                Debug.DrawLine(nodePos, roomPos, Color.yellow);
            }
        }
    }

    [Button]
    public void CreateDungeon()
    {
        if (seed != 0) Random.InitState(_seed);
        StopAllCoroutines();
        _roomsToSplit.Clear(); _roomsToDraw.Clear(); _doors.Clear();
        Destroy(WallsParent); Destroy(FloorParent);
        _graph.Clear(); 
        _navMeshSurface.RemoveData();

        _roomsToSplit.Add(_startRoom);
        drawCoroutine = StartCoroutine(DrawCoroutine());
    }

    IEnumerator DrawCoroutine()
    {
        //Rooms
        yield return new WaitForEndOfFrame();
        Debug.Log("coroutine start");

        while (_roomsToSplit.Count > 0)
        {
            RectInt currentRoom = _roomsToSplit[0];
            _roomsToSplit.Remove(currentRoom);

            int minRoomSizeRange = Random.Range((int)_minRoomSizeRage.x, (int)_minRoomSizeRage.y);

            if (currentRoom.width < minRoomSizeRange * 2 && currentRoom.height < minRoomSizeRange * 2)
            {
                _roomsToDraw.Add(currentRoom);
            }
            else SplitRooms(currentRoom);

            if (!SkipCoroutine) yield return new WaitForSeconds(0.1f);
        }

        //_doors
        for (int i = 0; i < _roomsToDraw.Count; i++)
        {
            for (int j = i + 1; j < _roomsToDraw.Count; j++)
            {
                if (AlgorithmsUtils.Intersects(_roomsToDraw[i], _roomsToDraw[j]))
                {
                    RectInt Inter = AlgorithmsUtils.Intersect(_roomsToDraw[i], _roomsToDraw[j]);
                    if ((Inter.width == 1 && Inter.height > 5) || (Inter.height == 1 && Inter.width > 5))
                    {
                        MakeDoor(_roomsToDraw[i], _roomsToDraw[j]);
                        if (!SkipCoroutine) yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
        if (_graph.BFS(_roomsToDraw[0])) Debug.Log("Rooms are all connected");
        else Debug.LogError("Rooms are all connected");

        Debug.Log("drawing is done; Total room count: " + _roomsToDraw.Count + "|  total Intersections: " + _doors.Count);

        //RemoveSmallest
        RemoveSmallest();
        if (!SkipCoroutine) yield return new WaitForSeconds(0.3f);

        //Assets
        Destroy(WallsParent);
        Destroy(FloorParent);
        _wallparents.Clear(); _walls.Clear();

        WallsParent = new GameObject("WallParent");
        FloorParent = new GameObject("ParentFloor");

        for (int i = 0; i < _doors.Count; i++)
        {
            _walls.Add(_doors[i].position);
            if (_doors[i].height > 1) _walls.Add(new Vector2(_doors[i].x, _doors[i].y + 1));
            else if (_doors[i].width > 1) _walls.Add(new Vector2(_doors[i].x + 1, _doors[i].y));
        }
        for (int i = 0; i < _roomsToDraw.Count; i++)
        {
            spawnroom(_roomsToDraw[i]);
            if (!SkipCoroutine) yield return new WaitForSeconds(0.1f);
        }

        CreateFloor();

        _navMeshSurface.BuildNavMesh();
        Player.GetComponent<PlayerController>().ResetDestination(new Vector3(_roomsToDraw[0].center.x, 5f, _roomsToDraw[0].center.y));

        Debug.Log("Done");
    }


    void SplitRooms(RectInt pRoom)
    {
        RectInt room1, room2; int splitRandom;

        if (pRoom.width >= pRoom.height)//vertical
        {
            splitRandom = Random.Range((int)_minRoomSizeRage.x, pRoom.width - (int)_minRoomSizeRage.x);

            room1 = new RectInt(pRoom.x, pRoom.y, splitRandom + 1, pRoom.height);
            room2 = new RectInt(pRoom.x + splitRandom, pRoom.y, pRoom.width - splitRandom, pRoom.height);
        }
        else//horizontal
        {
            splitRandom = Random.Range((int)_minRoomSizeRage.x, pRoom.height - (int)_minRoomSizeRage.x);

            room1 = new RectInt(pRoom.x, pRoom.y, pRoom.width, splitRandom + 1);
            room2 = new RectInt(pRoom.x, pRoom.y + splitRandom, pRoom.width, pRoom.height - splitRandom);

        }
        _roomsToSplit.Insert(0, room1); _roomsToSplit.Insert(1, room2);
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

        _doors.Add(door);
        _graph.AddEdge(door, room1); _graph.AddEdge(door, room2);
    }


    void RemoveSmallest()
    {
        //Remove rooms
        Dictionary<RectInt, int> roomSize = new Dictionary<RectInt, int>();
      
        for (int i = 0; i < _roomsToDraw.Count; i++)
        {
            roomSize.Add(_roomsToDraw[i], _roomsToDraw[i].width * _roomsToDraw[i].height);
        }
        bool connected = true;
        for (int i = 0; i < _roomsToDraw.Count/5 && connected; i++) // did 20% for a better effect
        {
            var minRoom = roomSize.OrderBy(kvp => kvp.Value).First(); //Got this line from the internet (Gets min value)
            List<RectInt> temp_doors = new List<RectInt>();

            foreach (var neighbor in _graph.GetNeighbors(minRoom.Key))
            {
                _graph.RemoveNode(neighbor);
                temp_doors.Add(neighbor);
            }
            _graph.RemoveNode(minRoom.Key);

            if (!_graph.BFS(roomSize.OrderByDescending(x => x.Value).First().Key)) //Got this line from the internet (Gets max value)
            {
                connected = false;
                Debug.Log("removal stopped");
            }
            else
            {
                foreach(var door in temp_doors)
                {
                    _doors.Remove(door);
                }
                _roomsToDraw.Remove(minRoom.Key);
                roomSize.Remove(minRoom.Key);
            }
            //Debug.Log($"\"{minRoom.Key}\" : \"{minRoom.Value}\"");
        }
    }

    // Assests
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public void SpawnDungeonAssets()
    {
        Destroy(WallsParent);
        Destroy(FloorParent);
        _wallparents.Clear(); _walls.Clear();

        WallsParent = new GameObject("WallParent");
        FloorParent = new GameObject("ParentFloor");

        for (int i = 0; i < _doors.Count; i++)
        {
            _walls.Add(_doors[i].position);
            if(_doors[i].height > 1) _walls.Add(new Vector2(_doors[i].x, _doors[i].y+1));
            else if (_doors[i].width > 1) _walls.Add(new Vector2(_doors[i].x+1, _doors[i].y));
        }
        for (int i = 0; i < _roomsToDraw.Count; i++)
        {
            spawnroom(_roomsToDraw[i]);
        }
        CreateFloor();
    }


    private void spawnroom(RectInt rectInt)
    {
        GameObject parentGameObject = new GameObject("Room: " + rectInt.position);
        _wallparents.Add(parentGameObject);

        for (int i = 0; i < rectInt.height; i++)
        {
            Vector2 postition = new Vector2(rectInt.x, rectInt.y + i);

            if (!_walls.Contains(postition))
            {
                WallPrefabsInstantiate(postition, parentGameObject);
            }
            postition = new Vector2(rectInt.x + rectInt.width - 1, rectInt.y + i);
            if (!_walls.Contains(postition))
            {
                WallPrefabsInstantiate(postition, parentGameObject);
            }
        }

        for (int i = 0; i < rectInt.width; i++)
        {
            Vector2 postition = new Vector2(rectInt.x + i, rectInt.y);
            if (!_walls.Contains(postition))
            {
                WallPrefabsInstantiate(postition, parentGameObject);
            }
            postition = new Vector2(rectInt.x + i, rectInt.y + rectInt.height - 1);
            if (!_walls.Contains(postition))
            {
                WallPrefabsInstantiate(postition, parentGameObject);
            }
        }
        parentGameObject.transform.position = new Vector3(0.5f, 0.5f, 0.5f);
        parentGameObject.transform.parent = WallsParent.transform;
    }

    
    public void WallPrefabsInstantiate(Vector2 Pos, GameObject Parent)
    {
        var newObject = Instantiate(WallPrefab, new Vector3(Pos.x, 0, Pos.y), Quaternion.identity, Parent.transform);
        newObject.name = "Wall: " + Pos;
        _walls.Add(Pos);
    }


    public void CreateFloor()
    {
        for (int i = 0; i < _roomsToDraw.Count; i++)
        {
            GameObject parentfloor = new GameObject("ParentFloor" + _roomsToDraw[i].position);
            parentfloor.transform.parent = FloorParent.transform;
            for (int j = 1; j < _roomsToDraw[i].width - 1; j++)
            {
                for (int k = 1; k < _roomsToDraw[i].height - 1; k++)
                {
                    
                    var newObject = Instantiate(FloorPrefab, new Vector3(j + _roomsToDraw[i].x, 0, k + _roomsToDraw[i].y), FloorPrefab.transform.rotation, parentfloor.transform);
                    newObject.name = "Floor: " + newObject.transform.position;
                }
            }
        }
        GameObject parentfloordoors = new GameObject("ParentFloor _doors");
        parentfloordoors.transform.parent = FloorParent.transform;

        for (int i = 0;i < _doors.Count; i++)
        {
            var newObject = Instantiate(FloorPrefab, new Vector3(_doors[i].x, 0, _doors[i].y), FloorPrefab.transform.rotation, parentfloordoors.transform);

            if(_doors[i].height > 1) Instantiate(FloorPrefab, new Vector3(_doors[i].x, 0, _doors[i].y + 1), FloorPrefab.transform.rotation, parentfloordoors.transform);
            else if (_doors[i].width > 1) Instantiate(FloorPrefab, new Vector3(_doors[i].x+1, 0, _doors[i].y), FloorPrefab.transform.rotation, parentfloordoors.transform);
            newObject.name = "Floor: " + newObject.transform.position;
        }
        FloorParent.transform.position = new Vector3(0.5f, 0, 0.5f);
    }

}