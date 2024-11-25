using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemVendingLoader : MonoBehaviour
{
    public GameObject panel; // Panel chứa các item
    public GameObject itemPrefab; // Prefab của item UI

    public List<Item> items = new List<Item>(); // Danh sách các item
    private PlayerHealth playerHealth;
    public GameObject spikeTrapPrefab;
    public GameObject gasTankPrefab;

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
            itemImage = Resources.Load<Sprite>("ImageItem/Health_potion"), // Load hình từ Resources
            price = 20,
            damageBoost = 0,
            itemDescription = "Recover 75% of the player's health"
        });

        items.Add(new Item
        {
            itemName = "Bullet Potion",
            itemImage = Resources.Load<Sprite>("ImageItem/Ammo_potion"), // Load hình từ Resources
            price = 20,
            damageBoost = 0,
            itemDescription = "Increase the player's ammo count by 30"
        });

        items.Add(new Item
        {
            itemName = "Gun Potion",
            itemImage = Resources.Load<Sprite>("ImageItem/Gun_Potion"), // Load hình từ Resources
            price = 20,
            damageBoost = 5,
            itemDescription = "Increase the player's weapon damage by 10"
        });

        items.Add(new Item
        {
            itemName = "Spike Trap",
            itemImage = Resources.Load<Sprite>("SpikeTrap_potion"), // Load hình từ Resources
            price = 20,
            damageBoost = 0,
            itemDescription = "A deadly trap that damages enemies"
        });
        items.Add(new Item
        {
            itemName = "Gas Tank",
            itemImage = Resources.Load<Sprite>("GasTank_potion"), // Load hình từ Resources
            price = 20,
            damageBoost = 0,
            itemDescription = "A deadly trap that damages enemies"
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
                            //playerHealth.Heal(50f);
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
                    if (item.itemName == "Spike Trap" || item.itemName == "Gas Tank")
                    {
                        GameObject player = GameObject.FindWithTag("Player"); // Giả sử Player có tag "Player"
                        if (player != null)
                        {
                            Vector3 spawnPosition = player.transform.position + player.transform.forward * 1f; // Tạo prefab cách player 1 đơn vị

                            // Raycast để tìm vị trí trên mặt đất
                            RaycastHit hit;
                            if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, Mathf.Infinity))
                            {
                                spawnPosition = hit.point; // Lấy điểm tiếp xúc với mặt đất
                                spawnPosition.y += 0.5f; // Đảm bảo rằng prefab được đặt cách mặt đất 1 đơn vị
                            }

                            // Khởi tạo prefab tại vị trí tính toán
                            GameObject trapPrefab = item.itemName == "Spike Trap" ? spikeTrapPrefab : gasTankPrefab;
                            GameObject trap = Instantiate(trapPrefab, spawnPosition, Quaternion.identity);
                            trap.SetActive(true); // Đảm bảo prefab được kích hoạt
                        }
                        else
                        {
                            Debug.LogError("Player không được tìm thấy.");
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
