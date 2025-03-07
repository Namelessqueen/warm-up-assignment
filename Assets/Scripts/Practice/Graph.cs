using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Graph<T>
{
    private Dictionary<T, List<T>> adjacencyList;

    public Graph() { adjacencyList = new Dictionary<T, List<T>>(); }
    public void AddNode(T node)
    {

    }
    public void AddEdge(T fromNode, T toNode)
    {

    }



}
