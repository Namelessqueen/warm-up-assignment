using System;
using System.Collections.Generic;
using UnityEngine;  
public class Graph<T>
{
    private Dictionary<T, List<T>> adjacencyList;

    public Graph() { adjacencyList = new Dictionary<T, List<T>>(); }
    public void AddNode(T node)
    {
        if(!adjacencyList.ContainsKey(node)) adjacencyList[node] = new List<T>();
    }
    public void AddEdge(T fromNode, T toNode)
    {
        if (!adjacencyList.ContainsKey(fromNode) || !adjacencyList.ContainsKey(toNode))
        {
            Debug.Log("One or both nodes do not exist in the graph.");
            return;
        }
        adjacencyList[fromNode].Add(toNode);
        adjacencyList[toNode].Add(fromNode);
    }

    public void Print()
    {
        foreach (var ele in adjacencyList)
        {
            Debug.Log($"Node: {ele.Key}, connections: {ele.Value}");
            foreach (var item in ele.Value)
            { Debug.Log($"connections: {item}"); }
        }
    }

    public List<T> GetNeighbors(T node)
    {

        if (!adjacencyList.ContainsKey(node))
        {
            Debug.Log("Node does not exist in the graph.");
        }        
        return adjacencyList[node];
    }
    
}
