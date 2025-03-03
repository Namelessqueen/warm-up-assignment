using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt room;
    public Vector2 minRoomSizeRage = new Vector2(10,20);
    
    Coroutine drawCoroutine;
    List<RectInt> roomsToDraw = new List<RectInt>();
    List<RectInt> roomsToSplit = new List<RectInt>();

    void Start()
    {
        roomsToSplit.Add(room);
        drawCoroutine = StartCoroutine(DrawCoroutine());
    }

    void Update()
    {
        AddRooms();
    }

    void AddRooms()
    {
        AlgorithmsUtils.DebugRectInt(room, Color.red);

        for (int i = 0; i < roomsToSplit.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(roomsToSplit[i], Color.red);
        }

        for (int i = 0; i < roomsToDraw.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(roomsToDraw[i], Color.green);
        }
    }

    IEnumerator DrawCoroutine()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("coroutine start");

        while (roomsToSplit.Count > 0)
        {
            RectInt currentRoom = roomsToSplit[0];
            roomsToSplit.Remove(currentRoom);

            int minRoomSizeRange = Random.Range((int)minRoomSizeRage.x, (int)minRoomSizeRage.y);

            if (currentRoom.width < minRoomSizeRange * 2&& currentRoom.height < minRoomSizeRange * 2)
            {
                roomsToDraw.Add(currentRoom);
            }
            else SplitRooms(currentRoom);

            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("drawing is done");
    }

    void SplitRooms(RectInt pRoom)
    {
        RectInt room1, room2; int splitRandom;

        if (pRoom.width>= pRoom.height)//vertical
        {
            splitRandom = Random.Range((int)minRoomSizeRage.x, pRoom.width - (int)minRoomSizeRage.x);
            
            room1 = new RectInt(pRoom.x, pRoom.y, splitRandom + 1, pRoom.height);
            room2 = new RectInt(pRoom.x + splitRandom , pRoom.y, pRoom.width - splitRandom, pRoom.height);
        }
       else//horizontal
       {
            splitRandom = Random.Range((int)minRoomSizeRage.x, pRoom.height - (int)minRoomSizeRage.x);

            room1 = new RectInt(pRoom.x, pRoom.y, pRoom.width, splitRandom + 1);
            room2 = new RectInt(pRoom.x, pRoom.y + splitRandom - 1, pRoom.width , pRoom.height - splitRandom + 1);
           
       }
        //Debug.Log(splitRandom);
        roomsToSplit.Add(room1); roomsToSplit.Add(room2);
    }
}
