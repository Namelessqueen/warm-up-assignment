using System.Collections.Generic;
using System;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public Dictionary<string, int> _inventory = new Dictionary<string, int>();
    void Start()
    {

    }


    void Update()
    {

    }

    public void AddItem(string Key, int Value = 1, bool isSpecial = false)
    {
        if (isSpecial && _inventory.ContainsKey(Key))
        {
            Debug.Log($"Special item {Key} already exists");
            return;
        }
        if (_inventory.ContainsKey(Key))
        {
            _inventory[Key] += Value;
        }
        else _inventory[Key] = Value;
    }
    public void RemoveItem(string Key, int Value = 1)
    {
        if (_inventory.ContainsKey(Key))
        {
            _inventory[Key] -= Value;

            if (_inventory[Key] <= 0)
            {
                _inventory.Remove(Key);
                Debug.Log($"item {Key} was removed");
            }
        }
    }

    public void display()
    {
        Console.WriteLine("Display");
        foreach (var ele in _inventory)
        {
            Debug.Log($"Key: {ele.Key}, Value: {ele.Value}");
        }
    }

    public bool HasItem(string itemName)
    {
        return _inventory.ContainsKey(itemName);
    }

}
