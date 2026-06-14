using TowerDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public class LevelButtonUI : MonoBehaviour
    {
        [Header("Level Config")]
        [Tooltip("Tên scene của màn chơi này (VD: Level1)")]
        [SerializeField] private string levelSceneName;
        [Tooltip("Tên scene của màn trước đó (để làm điều kiện mở). Bỏ trống nếu đây là màn 1.")]
        [SerializeField] private string previousLevelName;

        [Header("UI References")]
        [SerializeField] private Button buttonComponent;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject starsContainer;
        [SerializeField] private Image[] starImages; 
        [Header("Star Sprites")]
        [SerializeField] private Sprite starEarnedSprite;
        [SerializeField] private Sprite starEmptySprite;

        private void Start()
        {
            SetupButton();
        }

        private void SetupButton()
        {
            bool isUnlocked = SaveManager.IsLevelUnlocked(previousLevelName);

            if (buttonComponent != null) buttonComponent.interactable = isUnlocked;

            if (lockIcon != null) lockIcon.SetActive(!isUnlocked);

            if (starsContainer != null) starsContainer.SetActive(isUnlocked);

            if (isUnlocked)
            {
                int stars = SaveManager.GetLevelStars(levelSceneName);

                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                    {
                        // Nếu thứ tự của ngôi sao nhỏ hơn số sao đạt được -> Dùng sao Vàng, ngược lại dùng sao Trắng
                        starImages[i].sprite = (i < stars) ? starEarnedSprite : starEmptySprite;
                    }
                }
            }
        }

        public void LoadLevel()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            SceneManager.LoadScene(levelSceneName);
        }
    }
}
