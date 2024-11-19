using UnityEngine;
using UnityEngine.UI;

namespace Loading.UI
{
    public class LoadingBarController : MonoBehaviour
    {
        public Image loadingBarImage;

        public void Start()
        {
            HideLoadingBar();
        }

        public void ShowLoadingBar()
        {
            gameObject.SetActive(true);
        }

        public void HideLoadingBar()
        {
            gameObject.SetActive(false);
        }

        public void UpdateLoadingBar(float progress)
        {
            if (loadingBarImage != null)
            {
                loadingBarImage.fillAmount = progress;
            }

        }
    }
}