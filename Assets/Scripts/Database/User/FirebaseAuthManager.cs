using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class FirebaseAuthManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TMP_InputField usernameInput;
    public TextMeshProUGUI feedbackText;
    //public static string userId;
    private string apiKey = "AIzaSyA7kIAGmNwyJIJsS9x8uOjln8wDZ_wyDLo";

    //Hàm Register người dùng
    public void RegisterUser()
    {
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        string username = usernameInput.text;

        //Kiểm tra điều kiện đăng kí 
        if (password != confirmPassword)
        {
            feedbackText.text = "Password do not match!";
            return;
        }
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(username))
        {
            feedbackText.text = "Please input both email and password";
            return;
        }
        StartCoroutine(RegisterNewUser(email, password, username));
    }

    IEnumerator RegisterNewUser(string email, string password, string username)
    {
        string url = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" + apiKey;
        string json = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            feedbackText.text = "User registered successfully!";
            var responseData = JsonUtility.FromJson<FirebaseAuthRespone>(request.downloadHandler.text);

            // Lưu userId và idToken vào PlayerPrefs
            //PlayerPrefs.SetString("userId", responseData.localId);
            //PlayerPrefs.SetString("idToken", responseData.idToken);

            //// Lưu username vào PlayerPrefs
            //PlayerPrefs.SetString("username", username);
            //PlayerPrefs.SetString("email", email);
            //PlayerPrefs.Save();
            StartCoroutine(AddUserToDatabase(responseData.localId, email, username, 1000)); // Thêm vào Realtime Database
        }
        else
        {
            feedbackText.text = "Registration failed: " + request.error;
        }
    }

    // Thêm người dùng vào Realtime Database
    IEnumerator AddUserToDatabase(string userId, string email, string username, int coin)
    {
        string databaseUrl = "https://projectm-91ec6-default-rtdb.firebaseio.com/User/" + userId + ".json";
        User newUser = new User(email, username, "password", 1000);
        newUser.ownedSkins = new List<string>();
        newUser.selectedSkin = null;

        string json = JsonUtility.ToJson(newUser);
        UnityWebRequest request = new UnityWebRequest(databaseUrl, "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("User added to database successfully.");
        }
        else
        {
            Debug.LogError("Failed to add user to database: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }




    // Hàm đăng nhập người dùng
    public void LoginUser()
    {
        // Đặt currentUser về null trước khi đăng nhập
        Common.instance.currentUser = null;

        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Please input both email and password";
            return;
        }
        StartCoroutine(LoginExistingUser(email, password));
    }

    IEnumerator LoginExistingUser(string email, string password)
    {
        string url = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=" + apiKey;
        string json = "{\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"returnSecureToken\":true}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            feedbackText.text = "User logged in successfully!";
            var responseData = JsonUtility.FromJson<FirebaseAuthRespone>(request.downloadHandler.text);

            // Tiếp tục với thông tin người dùng sau khi đăng nhập thành công (vd: lưu token, localId...)
            PlayerPrefs.SetString("idToken", responseData.idToken);
            PlayerPrefs.SetString("userId", responseData.localId);
            PlayerPrefs.Save();

            // Gọi hàm GetCurrentUser để tải thông tin người dùng
            yield return StartCoroutine(Common.instance.GetCurrentUser());


            SceneManager.LoadScene("Menu");
        }
        else
        {
            feedbackText.text = "Login failed: " + request.error;
        }
    }
    public void LogoutUser()
    {
        // Xóa thông tin người dùng đã lưu trong PlayerPrefs
        PlayerPrefs.DeleteKey("idToken");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("email");
        PlayerPrefs.Save();

        // Đặt lại dữ liệu người dùng trong Common (nếu có)
        Common.instance.currentUser = null;

        // Chuyển về scene đăng nhập
        SceneManager.LoadScene("WelcomeScene"); // Đảm bảo "Login" là tên của scene đăng nhập
    }

}
