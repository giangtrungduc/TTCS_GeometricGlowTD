using TMPro;
using TowerDefense.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public class LevelSelectUI : ManagerBase<LevelSelectUI>
    {
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject informationPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeInfoButton;

        [Header("Thong tin level")]
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private Image iconLevelImage;
        [SerializeField] private Image levelBackgroundImage;
        [SerializeField] private Image[] startImages;
        [SerializeField] private Sprite filledStarSprite;
        [SerializeField] private Sprite emptyStarSprite;

        private LevelData selectedLevel;

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
            if (closeInfoButton != null) closeInfoButton.onClick.AddListener(OnCloseInfoButtonClicked);

            if (informationPanel != null)
            {
                informationPanel.SetActive(false);
            }

            RefreshPlayButton();
        }

        protected override void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayButtonClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            if (closeInfoButton != null) closeInfoButton.onClick.RemoveListener(OnCloseInfoButtonClicked);
            base.OnDestroy();
        }

        private void OnPlayButtonClicked()
        {
            if (selectedLevel == null) return;
            SceneLoader.TryLoadLevel(selectedLevel, this);
        }

        private void OnQuitButtonClicked()
        {
            SceneLoader.TryLoadScene(SceneLoader.MainMenuScene, this);
        }

        private void OnCloseInfoButtonClicked()
        {
            selectedLevel = null;
            if (informationPanel != null)
            {
                informationPanel.SetActive(false);
            }

            RefreshPlayButton();
        }

        public void ShowInformationPanel(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("[LevelSelectUI] LevelData bi null.", this);
                return;
            }

            selectedLevel = levelData;

            if (levelNameText != null)
            {
                levelNameText.text = levelData.levelName;
            }

            if (iconLevelImage != null)
            {
                iconLevelImage.sprite = levelData.iconLevel;
            }

            if (levelBackgroundImage != null)
            {
                levelBackgroundImage.sprite = levelData.backgroundLevel;
            }

            int stars = SaveManager.GetStars(levelData.levelID);
            for (int i = 0; i < startImages.Length; i++)
            {
                startImages[i].sprite = i < stars ? filledStarSprite : emptyStarSprite;
            }

            if (informationPanel != null)
            {
                informationPanel.SetActive(true);
            }

            RefreshPlayButton();
        }

        private void RefreshPlayButton()
        {
            if (playButton == null)
            {
                return;
            }

            bool canPlay = selectedLevel != null
                && selectedLevel.TryGetSceneIdentifier(out string sceneIdentifier)
                && SceneLoader.CanLoadScene(sceneIdentifier);

            playButton.interactable = canPlay;
        }
    }
}
