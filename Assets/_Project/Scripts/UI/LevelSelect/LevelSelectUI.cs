using TMPro;
using TowerDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectUI : ManagerBase<LevelSelectUI>
{
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject informationPanel;
    [SerializeField] private Button playButton;
    private string nameLevelToLoad;
    [SerializeField] private Button closeInfoButton;
    
    [Header("Thông tin level")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private Image iconLevelImage;
    [SerializeField] private Image levelBackgroundImage;
    [SerializeField] private Image[] startImages;
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;
    private void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
        closeInfoButton.onClick.AddListener(OnCloseInfoButtonClicked);
        informationPanel.SetActive(false);
    }
    private void OnPlayButtonClicked()
    {
        if (!string.IsNullOrEmpty(nameLevelToLoad))
        {
            SceneManager.LoadScene(nameLevelToLoad);
        }
    }
    private void OnQuitButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private void OnCloseInfoButtonClicked()
    {
        informationPanel.SetActive(false);
    }

    public void ShowInformationPanel(LevelData levelData)
    {
        levelNameText.text = levelData.levelName;
        iconLevelImage.sprite = levelData.iconLevel;
        levelBackgroundImage.sprite = levelData.backgroundLevel;

        int stars = SaveManager.GetStars(levelData.levelID);
        for (int i = 0; i < startImages.Length; i++)
        {
            startImages[i].sprite = i < stars ? filledStarSprite : emptyStarSprite;
        }
        nameLevelToLoad = levelData.levelName;
        informationPanel.SetActive(true);
    }
}
