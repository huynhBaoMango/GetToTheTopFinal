using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEditor.Progress;

public class ItemVendingLoader : MonoBehaviour
{
    public GameObject panel; // Panel chứa các item
    public GameObject itemPrefab; // Prefab của item UI

    public List<Item> items = new List<Item>(); // Danh sách các item
    private PlayerHealth playerHealth;

    void Start()
    { 
        playerHealth = FindObjectOfType<PlayerHealth>();
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
            price = 20,
            damageBoost = 0,
            itemDescription = "Recover 75% of the player's health"
        });

        items.Add(new Item
        {
            itemName = "Bullet Potion",
            itemImage = Resources.Load<Sprite>("ImageItem/Bullet_Potion"), // Load hình từ Resources
            price = 20,
            damageBoost = 0,
            itemDescription = "Increase the player's ammo count by 30"
        });

        items.Add(new Item
        {
            itemName = "Gun Potion",
            itemImage = Resources.Load<Sprite>("ImageItem/Gun_Potion"), // Load hình từ Resources
            price = 20,
            damageBoost = 20,
            itemDescription = "Increase the player's weapon damage by 10"
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

            Button buyButton = newItem.GetComponentInChildren<Button>();
            if(buyButton != null)
            {
                buyButton.onClick.AddListener(() =>
                {
                    if (playerHealth != null)
                    {
                        if (item.itemName == "Health Potion")
                        {
                            playerHealth.Heal(20f);
                        }
                    }
                    if (item.itemName == "Bullet Potion")
                    {
                        APlayerWeapon currentWeapon = FindObjectOfType<APlayerWeapon>();
                        if (currentWeapon != null)
                        {
                            currentWeapon.maxAmmo = Mathf.Min(currentWeapon.initialMaxAmmo, currentWeapon.maxAmmo + 30);
                        }
                    }
                    if (item.itemName == "Gun Potion")
                    {
                        // Tăng sát thương cho tất cả vũ khí của người chơi
                        foreach (APlayerWeapon weapon in FindObjectsOfType<APlayerWeapon>())
                        {
                            weapon.damage += item.damageBoost;
                            Debug.Log("Damage: " + weapon.damage);
                        }
                    }
                });
            }
            else
            {
                Debug.LogWarning("Button component not found in itemPrefab.");
            }
        }
    }
}
