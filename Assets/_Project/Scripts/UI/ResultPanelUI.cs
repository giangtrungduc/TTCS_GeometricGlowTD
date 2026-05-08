using TMPro;
using TowerDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public class ResultPanelUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Result")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Image resultImage;
        [SerializeField] private Sprite victorySprite;
        [SerializeField] private Sprite defeatSprite;

        [Header("Stars")]
        [SerializeField] private Image[] stars;
        [SerializeField] private Sprite filledStarSprite;
        [SerializeField] private Sprite emptyStarSprite;

        [Header("Buttons")]
        [SerializeField] private Button replayButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button nextButton;

        private void Awake()
        {
            panelRoot?.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnLevelCompleted += HandleLevelCompleted;

            replayButton?.onClick.AddListener(OnReplayClicked);
            homeButton?.onClick.AddListener(OnHomeClicked);
            nextButton?.onClick.AddListener(OnNextClicked);
        }

        private void OnDisable()
        {
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;

            replayButton?.onClick.RemoveListener(OnReplayClicked);
            homeButton?.onClick.RemoveListener(OnHomeClicked);
            nextButton?.onClick.RemoveListener(OnNextClicked);
        }

        private void HandleLevelCompleted(LevelResult result)
        {
            panelRoot?.SetActive(true);

            bool isVictory = result != null && result.isVictory;
            int starCount = result != null ? Mathf.Clamp(result.starCount, 0, 3) : 0;

            SetupUI(isVictory, starCount);
        }

        private void SetupUI(bool isVictory, int starCount)
        {
            if (resultText != null)
            {
                resultText.text = isVictory ? "VICTORY" : "DEFEAT";
            }

            if (resultImage != null)
            {
                resultImage.sprite = isVictory ? victorySprite : defeatSprite;
            }

            UpdateStars(starCount);
            SetupButtons(isVictory);
        }

        private void UpdateStars(int starCount)
        {
            if (stars == null) return;

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].sprite = i < starCount ? filledStarSprite : emptyStarSprite;
            }
        }

        private void SetupButtons(bool isVictory)
        {
            bool hasNext = HasNextScene();

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(isVictory && hasNext);
            }

            replayButton?.gameObject.SetActive(true);
            homeButton?.gameObject.SetActive(true);
        }

        private bool HasNextScene()
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            return next < SceneManager.sceneCountInBuildSettings;
        }

        private void OnReplayClicked()
        {
            GameManager.Instance?.RestartLevel();
        }

        private void OnHomeClicked()
        {
            GameManager.Instance?.QuitToLevelSelect();
        }

        private void OnNextClicked()
        {
            if (!HasNextScene()) return;

            Time.timeScale = 1f;
            string nextScenePath = SceneUtility.GetScenePathByBuildIndex(SceneManager.GetActiveScene().buildIndex + 1);
            SceneLoader.TryLoadScene(nextScenePath, this);
        }
    }
}
