using UnityEngine;

public class GraphTester : MonoBehaviour
{
    Graph<string> graph = new Graph<string>();
    void Start()
    {
        graph.AddNode("A");
        graph.AddNode("B");
        graph.AddNode("C");
        graph.AddNode("D");
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");
        graph.AddEdge("B", "D");
        graph.AddEdge("C", "D");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Graph Structure:");
            //graph.Print();
            Debug.Log(graph.GetNeighbors("A"));
        }
    }
}
