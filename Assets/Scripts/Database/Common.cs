using IO.Swagger.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Common : MonoBehaviour
{
    public static Common instance;
    public User currentUser;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Đảm bảo rằng object không bị hủy khi chuyển scene
        }
        else
        {
            Destroy(gameObject); // Hủy object này nếu đã có instance khác tồn tại
        }
    }

    //Ham mua skin
    public bool PurchaseSkin(User user, Skin skin)
    {
        if (user.ownedSkins == null)
        {
            Debug.LogError("ownedSkins is null!");
            user.ownedSkins = new List<string>(); // Khởi tạo nếu null
        }

        if (user.ownedSkins.Contains(skin.skinId))
        {
            Debug.Log("User already owns this skin!");
            return false;
        }

        if (user.coin < skin.price)
        {
            Debug.Log("Not enough coins to purchase this skin!");
            return false;

        }

        user.coin -= skin.price;  //Giam coin cua User
        user.ownedSkins.Add(skin.skinId);  //Them skin da mua vao nguoi dung
        Debug.Log($"User's owned skins: {string.Join(", ", user.ownedSkins)}");
        skin.isPurchased = true;

        // Cập nhật dữ liệu người dùng vào Database sau khi mua skin
        UpdateUserInDatabase(user);

        Debug.Log($"Skin {skin.skinName} purchased successfully!");
        return true;
    }



    //Ham chon skin
    public bool SelectSkin(User user, Skin skin)
    {
        if (!user.ownedSkins.Contains(skin.skinId))
        {
            Debug.Log("User doesn't own this skin");
            return false;
        }

        //Gan skin da chon cho user
        user.selectedSkin = skin.skinId;
        // Cập nhật dữ liệu người dùng
        UpdateUserInDatabase(user);


        Debug.Log($"{skin.skinName} selected successfully");
        return true;
    }


    // Hàm cập nhật thông tin người dùng trong Firebase
    public void UpdateUserInDatabase(User user)
    {
        //string currentUserId = FirebaseAuthManager.userId;
        string userId = PlayerPrefs.GetString("userId");
        string databaseUrl = "https://projectm-91ec6-default-rtdb.firebaseio.com/User/" + userId + ".json";
        string json = JsonUtility.ToJson(user);

        StartCoroutine(UpdateUserCoroutine(databaseUrl, json));
    }

    private IEnumerator UpdateUserCoroutine(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("User updated in database successfully.");
        }
        else
        {
            Debug.LogError("Failed to update user in database: " + request.error);
        }
    }

    // Hàm tải skins từ Firebase
    public void LoadSkinsFromDatabase(Action<List<Skin>> onSkinsLoaded)
    {
        string databaseUrl = "https://projectm-91ec6-default-rtdb.firebaseio.com/Skin.json"; // URL đến danh sách skins
        StartCoroutine(LoadSkinsCoroutine(databaseUrl, onSkinsLoaded));
    }

    // Coroutine để tải skins từ Firebase
    private IEnumerator LoadSkinsCoroutine(string url, Action<List<Skin>> onSkinsLoaded)
    {
        // Tạo yêu cầu GET đến Firebase
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Chuyển đổi JSON trả về thành danh sách Skin
            string json = request.downloadHandler.text;
            Debug.Log("Received JSON: " + json);

            // Sử dụng Newtonsoft.Json để chuyển đổi
            var skinDict = JsonConvert.DeserializeObject<Dictionary<string, Skin>>(json);
            if (skinDict != null)
            {
                List<Skin> skins = new List<Skin>(skinDict.Values);
                foreach (var entry in skinDict)
                {
                    entry.Value.skinId = entry.Key; // Gán skinId từ khóa của đối tượng
                }
                onSkinsLoaded?.Invoke(skins);   // Gọi callback với danh sách skins đã tải
            }
            else
            {
                Debug.LogError("Failed to parse skin dictionary from JSON.");
                onSkinsLoaded?.Invoke(new List<Skin>()); // Gọi callback với danh sách rỗng nếu có lỗi
            }
        }
        else
        {
            Debug.LogError("Failed to load skins: " + request.error);
            onSkinsLoaded?.Invoke(new List<Skin>()); // Gọi callback với danh sách rỗng nếu có lỗi
        }
    }

    // Coroutine để lấy thông tin người dùng từ Firebase
    public IEnumerator GetCurrentUser()
    {
        //string currentUserId = FirebaseAuthManager.userId;
        string userId = PlayerPrefs.GetString("userId");
        Debug.Log("id user: " + userId);
        string databaseUrl = "https://projectm-91ec6-default-rtdb.firebaseio.com/User/" + userId + ".json";

        UnityWebRequest request = UnityWebRequest.Get(databaseUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Getting current user from database...");
            // Chuyển đổi JSON trả về thành đối tượng User
            currentUser = JsonUtility.FromJson<User>(request.downloadHandler.text);
            if (currentUser != null)
            {
                Debug.Log("User data retrieved successfully.");
                Debug.Log($"Owned Skins: {string.Join(", ", currentUser.ownedSkins)}");
            }
            else
            {
                Debug.LogError("Không thể phân tích dữ liệu user từ JSON.");
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve user data: " + request.error);
            currentUser = null; // Nếu có lỗi, trả về null
        }
    }

    // Coroutine để mua skin
    private IEnumerator PurchaseSkinCoroutine(User user, Skin skin)
    {
        if (skin == null)
        {
            Debug.LogError("Skin is null!");
            yield break; // Dừng coroutine nếu skin không hợp lệ
        }
        Debug.Log($"Attempting to purchase skin with ID: {skin.skinId}");

        if (PurchaseSkin(user, skin))
        {
            // Cập nhật thông tin người dùng vào Firebase sau khi mua skin thành công
            UpdateUserInDatabase(user);

            Debug.Log($"{skin.skinName} purchased successfully!");
        }
        else
        {
            // Nếu không mua được skin, có thể hiển thị thông báo cho người dùng
            Debug.Log($"Failed to purchase {skin.skinName}. Check the console for details.");
        }

        yield return null; // Kết thúc coroutine
    }

    // Coroutine để lấy thông tin người dùng và thực hiện mua skin
    public IEnumerator GetCurrentUserAndPurchase(Skin skin)
    {

        // Gọi coroutine để lấy thông tin người dùng
        yield return StartCoroutine(GetCurrentUser());

        // Kiểm tra xem thông tin người dùng có hợp lệ không
        if (currentUser != null)
        {
            // Gọi coroutine để mua skin
            yield return StartCoroutine(PurchaseSkinCoroutine(currentUser, skin));
        }
        else
        {
            Debug.Log("User data is invalid. Cannot purchase skin.");
        }
    }
}
