using TowerDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public class ResultUIController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        
        [Header("Stars")]
        [Tooltip("Mảng 3 Image chứa 3 ngôi sao")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite yellowStarSprite;
        [SerializeField] private Sprite whiteStarSprite;

        [Header("Buttons")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button nextButton;

        private void Start()
        {
            if (resultPanel != null) resultPanel.SetActive(false);

            if (homeButton != null) homeButton.onClick.AddListener(OnHomeClicked);
            if (replayButton != null) replayButton.onClick.AddListener(OnReplayClicked);
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
            
            // Đăng ký sự kiện (Lắng nghe GameManager)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += HandleDefeat;
                GameManager.Instance.OnVictory += HandleVictory;
            }
        }

        private void OnDestroy()
        {
            // Hủy đăng ký sự kiện khi đổi Scene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver -= HandleDefeat;
                GameManager.Instance.OnVictory -= HandleVictory;
            }
        }

        private void HandleDefeat()
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            
            if (resultText != null)
            {
                resultText.text = "THẤT BẠI";
                resultText.color = Color.red; // Đổi chữ màu Đỏ
            }

            // Tất cả sao thành màu trắng
            UpdateStars(0);

            // Ẩn nút Next vì thua thì không được đi tiếp
            if (nextButton != null) nextButton.gameObject.SetActive(false);
        }

        private void HandleVictory(int stars)
        {
            if (resultPanel != null) resultPanel.SetActive(true);
            
            if (resultText != null)
            {
                resultText.text = "CHIẾN THẮNG";
                resultText.color = Color.green; // Đổi chữ màu Xanh lá
            }

            // Hiển thị số sao Vàng
            UpdateStars(stars);

            // Xử lý nút Next: Chỉ hiện nếu GameManager có khai báo tên bài tiếp theo
            if (nextButton != null)
            {
                if (!string.IsNullOrEmpty(GameManager.Instance.NextLevelName))
                {
                    nextButton.gameObject.SetActive(true);
                }
                else
                {
                    nextButton.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateStars(int yellowCount)
        {
            if (starImages == null || starImages.Length == 0) return;

            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    if (i < yellowCount)
                    {
                        starImages[i].sprite = yellowStarSprite;
                    }
                    else
                    {
                        starImages[i].sprite = whiteStarSprite;
                    }
                }
            }
        }

        // --- BUTTON EVENTS ---

        private void OnHomeClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            Time.timeScale = 1f; // Bỏ đóng băng
            SceneManager.LoadScene("LevelSelect");
        }

        private void OnReplayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            Time.timeScale = 1f; // Bỏ đóng băng
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnNextClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            Time.timeScale = 1f; // Bỏ đóng băng
            
            if (!string.IsNullOrEmpty(GameManager.Instance.NextLevelName))
            {
                SceneManager.LoadScene(GameManager.Instance.NextLevelName);
            }
            else
            {
                // Backup an toàn: Nhỡ lỗi hiện nút mà không có bài Next thì quay về Home
                SceneManager.LoadScene("LevelSelect");
            }
        }
    }
}
