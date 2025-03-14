using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt startRoom;
    public Vector2 minRoomSizeRage = new Vector2(10, 20);

    Coroutine drawCoroutine;
    List<RectInt> roomsToDraw = new List<RectInt>();
    List<RectInt> roomsToSplit = new List<RectInt>();
    List<RectInt> Doors = new List<RectInt>();
    List<RectInt> DoorIntersections = new List<RectInt>();

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
        DebugExtension.DebugCircle(new Vector3(startRoom.x + (startRoom.width/2),0, startRoom.y+(startRoom.height/2)), Color.red, 10);
        
        AlgorithmsUtils.DebugRectInt(startRoom, Color.red);

        for (int i = 0; i < roomsToSplit.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(roomsToSplit[i], Color.red);
            
        }

        for (int i = 0; i < roomsToDraw.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(roomsToDraw[i], Color.green);
        }

        for (int i = 0; i < Doors.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(Doors[i], Color.cyan);
        }
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

            yield return new WaitForSeconds(0.1f);
        }

        //Doors
        for (int i = 0; i < roomsToDraw.Count; i++)
        {
            for (int j = i + 1; j < roomsToDraw.Count; j++)
            {
                if (AlgorithmsUtils.Intersects(roomsToDraw[i], roomsToDraw[j]))
                {
                    RectInt Inter = AlgorithmsUtils.Intersect(roomsToDraw[i], roomsToDraw[j]);
                    if ((Inter.width == 1 && Inter.height > 5) || (Inter.height == 1 && Inter.width > 5))
                    {
                        DoorIntersections.Add(Inter);
                    }
                }
            }
        }
        for (int i = 0; i < DoorIntersections.Count; i++)
        {
            MakeDoor(DoorIntersections[i]);
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("drawing is done; Total room count: " + roomsToDraw.Count + "|  total Intersections: " + DoorIntersections.Count);

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

    void MakeDoor(RectInt intersect)
    {
        RectInt door = new RectInt(0, 0, 0, 0); int positionRandom;
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
    }
}