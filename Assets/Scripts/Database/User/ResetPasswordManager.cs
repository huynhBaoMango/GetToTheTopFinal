using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PasswordResetManager : MonoBehaviour
{
    public TMP_InputField emailInputField;  // InputField để người dùng nhập email
    public TextMeshProUGUI feedbackText;  // Text để hiển thị thông báo cho người dùng

    private string apiKey = "AIzaSyA7kIAGmNwyJIJsS9x8uOjln8wDZ_wyDLo";

    // Hàm gửi yêu cầu khôi phục mật khẩu
    public void SendPasswordResetEmail()
    {
        StartCoroutine(ResetPassword(emailInputField.text));
    }

    IEnumerator ResetPassword(string email)
    {
        string url = "https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key=" + apiKey;
        string json = "{\"requestType\":\"PASSWORD_RESET\",\"email\":\"" + email + "\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            feedbackText.text = "Password reset email sent!";
        }
        else
        {
            feedbackText.text = "Error sending password reset email: " + request.error;
            Debug.LogError("Error: " + request.downloadHandler.text);
        }
    }
}
