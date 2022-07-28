
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
    private static float cooldown = 0.5f;

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
        current_text.text = items[currently_selected_item];
        next_text.text = items[ItemManager.NextItem()];
        prev_text.text = items[ItemManager.PrevItem()];

        cooldown -= Time.deltaTime;
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
            return 0;
        } else {
            return currently_selected_item + 1;
        }
    }

    public static int PrevItem() {
        if(currently_selected_item == 0) {
            return items.Length - 1;
        } else {
            return currently_selected_item - 1;
        }
    }

    public static void SwitchItem(bool next) {
        if(cooldown <= 0) {
            if(next) {
                currently_selected_item = NextItem();
            } else {
                currently_selected_item = PrevItem();
            }
            cooldown = 0.5f;
        }
    }
}