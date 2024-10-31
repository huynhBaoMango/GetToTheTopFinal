using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinModelLoader : MonoBehaviour
{
    public GameObject skinModelPrefab;  // Đối tượng cha nơi mô hình skin sẽ được tải
    public Button btnBuy, btnSelect, btnPrev, btnNext;
    public TextMeshProUGUI btnBuyText, btnSelectText, txtUserCoin, txtSkinName, txtUsername;
    private int currentSkinIndex = 0;
    private List<Skin> skins;
    private Skin currentSkin;
    public float rotationSpeed = 250f;
    // Start is called before the first frame update
    private IEnumerator Start()
    {
        // Chờ dữ liệu người dùng được tải từ Firebase
        yield return StartCoroutine(Common.instance.GetCurrentUser());

        // Kiểm tra nếu có dữ liệu người dùng
        if (Common.instance.currentUser != null)
        {
            Debug.Log("Current user: " + Common.instance.currentUser.username);
            txtUserCoin.text = "" + Common.instance.currentUser.coin;
            txtUsername.text = Common.instance.currentUser.username;
        }
        else
        {
            Debug.LogError("Không lấy được dữ liệu người dùng hiện tại.");
        }

        // Tải danh sách skins từ cơ sở dữ liệu
        Common.instance.LoadSkinsFromDatabase(OnSkinsLoaded);

        // Gán sự kiện cho các nút
        btnPrev.onClick.AddListener(ShowPreviousSkin);
        btnNext.onClick.AddListener(ShowNextSkin);
        btnBuy.onClick.AddListener(BuyCurrentSkin);
        btnSelect.onClick.AddListener(SelectCurrentSkin);
    }

    private void OnSkinsLoaded(List<Skin> loadedSkins)
    {
        skins = loadedSkins;
        foreach (var skin in skins)
        {
            Debug.Log($"Loaded skin ID: {skin.skinId}, Name: {skin.skinName}, Price: {skin.price}");
        }
        UpdateSkinDisplay();
    }

    private void UpdateSkinDisplay()
    {


        if (skins == null || skins.Count == 0) return;

        // Lấy skin hiện tại dựa vào chỉ số
        currentSkin = skins[currentSkinIndex];
        txtSkinName.text = currentSkin.skinName;



        // Tải mô hình 3D
        foreach (Transform child in skinModelPrefab.transform)
        {
            Destroy(child.gameObject); // Xóa mô hình cũ
        }
        GameObject skinModel = Instantiate(Resources.Load<GameObject>(currentSkin.assetPath), skinModelPrefab.transform);
        skinModel.transform.localScale = new Vector3(300, 300, 300);
        skinModel.transform.localPosition = new Vector3(0, -300, 0);
        skinModel.transform.localRotation = Quaternion.Euler(0, 180, 0);

        // Cập nhật trạng thái các nút
        UpdateButtonStates();


        Debug.Log("Skin đã mua: " + Common.instance.currentUser.ownedSkins);
        Debug.Log($"User's owned skins: {string.Join(", ", Common.instance.currentUser.ownedSkins)}");
    }

    private void UpdateButtonStates()
    {
        if (currentSkin == null)
        {
            Debug.LogError("Current skin is null.");
            return; // Không tiếp tục nếu currentSkin là null
        }

        // Nút Mua
        if (UserOwnsSkin(currentSkin))
        {
            btnBuyText.text = "Đã mua";
            btnBuy.interactable = false;
            btnBuy.image.color = Color.gray;  // Màu mờ
        }
        else
        {
            btnBuyText.text = "Mua " + currentSkin.price + "$";
            btnBuy.interactable = true;
            btnBuy.image.color = Color.white;  // Màu bình thường
        }

        // Nút Chọn
        if (UserOwnsSkin(currentSkin))
        {
            if (UserSelectSkin(currentSkin))
            {
                btnSelectText.text = "Đã chọn";
                btnSelect.interactable = false;
                btnSelect.image.color = Color.gray;
            }
            else
            {
                btnSelectText.text = "Chọn";
                btnSelect.interactable = true;
                btnSelect.image.color = Color.white;
            }
        }
        else
        {
            btnSelectText.text = "Chưa sở hữu";
            btnSelect.interactable = false;
            btnSelect.image.color = Color.gray;  // Màu mờ
        }
    }

    private void ShowPreviousSkin()
    {
        //txtSkinName.text = currentSkin.skinName;
        currentSkinIndex = (currentSkinIndex - 1 + skins.Count) % skins.Count;
        Debug.Log("Skin " + currentSkin.skinName);
        UpdateSkinDisplay();
    }

    private void ShowNextSkin()
    {
        //txtSkinName.text = currentSkin.skinName;
        currentSkinIndex = (currentSkinIndex + 1) % skins.Count;
        Debug.Log("Skin " + currentSkin.skinName);
        UpdateSkinDisplay();
    }

    private void BuyCurrentSkin()
    {
        StartCoroutine(BuyCurrentSkinCoroutine());
    }
    private IEnumerator BuyCurrentSkinCoroutine()
    {
        yield return StartCoroutine(Common.instance.GetCurrentUserAndPurchase(currentSkin));

        // Kiểm tra lại sau khi mua để cập nhật số coin nếu thành công
        if (Common.instance.currentUser != null)
        {
            Debug.Log($"Số coin sau khi mua: {Common.instance.currentUser.coin}");
            txtUserCoin.text = "" + Common.instance.currentUser.coin;
            UpdateSkinDisplay();
        }
    }

    private void SelectCurrentSkin()
    {
        if (UserOwnsSkin(currentSkin))
        {
            // Logic chọn skin
            Common.instance.SelectSkin(Common.instance.currentUser, currentSkin);
            Debug.Log($"{currentSkin.skinName} đã được chọn.");
        }
        UpdateSkinDisplay();
    }

    private bool UserOwnsSkin(Skin skin)
    {
        if (Common.instance.currentUser == null || Common.instance.currentUser.ownedSkins == null)
        {
            Debug.LogError("Current user or ownedSkins is null.");
            return false; // Trả về false nếu không có dữ liệu người dùng
        }
        return Common.instance.currentUser.ownedSkins.Contains(skin.skinId);
    }
    private bool UserSelectSkin(Skin skin)
    {
        return Common.instance.currentUser.selectedSkin.Contains(skin.skinId);
    }

    void Update()
    {
        ModelRotator();
    }

    void ModelRotator()
    {
        // Xoay mô hình
        if (Input.GetMouseButton(0))
        {
            //skinModelPrefab.transform.GetChild(0).Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            skinModelPrefab.transform.GetChild(0).Rotate(Vector3.up, -rotationX, Space.World);
        }
    }
}
