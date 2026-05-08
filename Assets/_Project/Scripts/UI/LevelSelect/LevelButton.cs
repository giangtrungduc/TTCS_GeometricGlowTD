using TowerDefense.Core;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Button levelButton;

        [SerializeField] private LevelData levelData;
        [SerializeField] private Image lockIcon;
        [Tooltip("Các hình ảnh sao dùng để hiển thị số sao đạt được của level")]
        [SerializeField] private Image[] startImages;
        [SerializeField] private Sprite filledStarSprite;
        [SerializeField] private Sprite emptyStarSprite;


    private void Start()
    {
        levelButton.onClick.AddListener(OnLevelButtonClicked);
        if (!SaveManager.IsLevelUnlocked(levelData.levelID))
        {
            lockIcon.gameObject.SetActive(true);
            levelButton.interactable = false;
            foreach (var starImage in startImages)
            {
                starImage.gameObject.SetActive(false);
            }
        }
        else
        {
            lockIcon.gameObject.SetActive(false);
            levelButton.interactable = true;
            foreach (var starImage in startImages)
            {
                starImage.gameObject.SetActive(true);
            }

            int stars = SaveManager.GetStars(levelData.levelID);
            for (int i = 0; i < startImages.Length; i++)
            {
                startImages[i].sprite = i < stars ? filledStarSprite : emptyStarSprite;
            }
        }
    }
    private void OnLevelButtonClicked()
    {
        LevelSelectUI.Instance.ShowInformationPanel(levelData);
    }
}
