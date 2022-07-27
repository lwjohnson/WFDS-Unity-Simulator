
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public GameObject current;
    public GameObject next;
    public GameObject prev;

    public static Text current_text;
    public static Text next_text;
    public static Text prev_text;
    public static int currently_selected_item = 0;

    private static string[] items = new string[] { "Fire", "Trees", "Trenches" };


    void Start()
    {
        current_text = current.GetComponent<Text>();
        next_text = next.GetComponent<Text>();
        prev_text = prev.GetComponent<Text>();
        
        current_text.text = items[currently_selected_item];
        next_text.text = items[ItemManager.NextItem()];
        prev_text.text = items[ItemManager.PrevItem()];
    }

    // Update is called once per frame
    void Update()
    {
        current_text.text = "Current: ";
        next_text.text = "Next: ";
        prev_text.text = "Prev: ";
    }

    public static int GetCurrentlySelectedItem()
    {
        return currently_selected_item;
    }

    public static void SetCurrentlySelectedItem(int item)
    {
        currently_selected_item = item;
    }

    public static string[] GetItems()
    {
        return items;
    }

    public static int NextItem() {
        if(currently_selected_item == items.Length - 1) {
            currently_selected_item = 0;
        } else {
            currently_selected_item++;
        }
        return currently_selected_item;
    }

    public static int PrevItem() {
        if(currently_selected_item == 0) {
            currently_selected_item = items.Length - 1;
        } else {
            currently_selected_item--;
        }
        return currently_selected_item;
    }
}