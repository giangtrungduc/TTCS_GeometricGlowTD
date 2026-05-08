using TowerDefense.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Button levelButton;
        [SerializeField] private LevelData levelData;
        [SerializeField] private Image lockIcon;
        [SerializeField] private Image[] startImages;
        [SerializeField] private Sprite filledStarSprite;
        [SerializeField] private Sprite emptyStarSprite;

        private void Start()
        {
            if (levelButton != null)
            {
                levelButton.onClick.AddListener(OnLevelButtonClicked);
            }

            RefreshState();
        }

        private void OnDestroy()
        {
            if (levelButton != null)
            {
                levelButton.onClick.RemoveListener(OnLevelButtonClicked);
            }
        }

        private void RefreshState()
        {
            if (levelData == null || levelButton == null)
            {
                Debug.LogError("[LevelButton] Thieu LevelData hoac Button reference.", this);
                return;
            }

            bool isUnlocked = SaveManager.IsLevelUnlocked(levelData.levelID);
            bool hasValidScene = levelData.TryGetSceneIdentifier(out string sceneIdentifier)
                && SceneLoader.CanLoadScene(sceneIdentifier);

            if (lockIcon != null)
            {
                lockIcon.gameObject.SetActive(!isUnlocked || !hasValidScene);
            }

            levelButton.interactable = isUnlocked && hasValidScene;

            bool showStars = isUnlocked;
            for (int i = 0; i < startImages.Length; i++)
            {
                startImages[i].gameObject.SetActive(showStars);
            }

            if (!showStars)
            {
                return;
            }

            int stars = SaveManager.GetStars(levelData.levelID);
            for (int i = 0; i < startImages.Length; i++)
            {
                startImages[i].sprite = i < stars ? filledStarSprite : emptyStarSprite;
            }

            if (!hasValidScene)
            {
                Debug.LogWarning($"[LevelButton] Level '{levelData.name}' chua co scene hop le trong build.", this);
            }
        }

        private void OnLevelButtonClicked()
        {
            if (LevelSelectUI.Instance != null)
            {
                LevelSelectUI.Instance.ShowInformationPanel(levelData);
            }
        }
    }
}
