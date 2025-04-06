using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    public string item = "Coins";
    InventorySystem inventorySystem;
    void Start()
    {
        inventorySystem = GetComponent<InventorySystem>();
        inventorySystem.AddItem("Potion");
        inventorySystem.AddItem("Coins", 5);
        inventorySystem.AddItem("Potion");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            inventorySystem.display();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            inventorySystem.AddItem(item);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventorySystem.RemoveItem(item);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log(inventorySystem.Search(item));
        }
    }
}
