using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemVendingLoader : MonoBehaviour
{
    public GameObject panel; // Panel chứa các item
    public GameObject itemPrefab; // Prefab của item UI

    public List<Item> items = new List<Item>(); // Danh sách các item

    void Start()
    {
        InitializeItems();
        LoadItemsToPanel();
    }

    void InitializeItems()
    {
        // Thêm item vào danh sách
        items.Add(new Item
        {
            itemName = "Health Potion",
            itemImage = Resources.Load<Sprite>("ImageItem/Health_Potion"), // Load hình từ Resources
            price = 20
        });

        items.Add(new Item
        {
            itemName = "Bullet Potion",
            itemImage = Resources.Load<Sprite>("ImageItem/Bullet_Potion"), // Load hình từ Resources
            price = 20
        });

        items.Add(new Item
        {
            itemName = "Gun Potion",
            itemImage = Resources.Load<Sprite>("ImageItem/Gun_Potion"), // Load hình từ Resources
            price = 20
        });
    }

        void LoadItemsToPanel()
    {
        foreach (Item item in items)
        {
            // Tạo một item mới trên Panel
            GameObject newItem = Instantiate(itemPrefab, panel.transform);

            // Set thông tin của item
            //TextMeshProUGUI itemNameText = newItem.GetComponentInChildren<TextMeshProUGUI>();
            //if (itemNameText != null)
            //{
            //    itemNameText.text = $"{item.itemName}\nPrice: {item.price}";
            //}

            TextMeshProUGUI itemPriceText = newItem.GetComponentInChildren<TextMeshProUGUI>();
            if(itemPriceText != null)
                itemPriceText.text = "$" + item.price;


            Image itemImage = newItem.GetComponentInChildren<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = item.itemImage;
            }
        }
    }
}
