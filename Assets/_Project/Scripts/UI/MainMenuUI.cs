using TowerDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu")]
        [Tooltip("Button bắt đầu trò chơi")]
        [SerializeField] private Button startButton;
        [Tooltip("Button mở cài đặt")]
        [SerializeField] private Button settingsButton;
        [Tooltip("Button thoát trò chơi")]
        [SerializeField] private Button quitButton;

        [Header("Settings Menu")]
        [Tooltip("Panel cài đặt")]
        [SerializeField] private GameObject settingsPanel;
        [Tooltip("Slider điều chỉnh âm lượng nhạc")]
        [SerializeField] private Slider musicVolumeSlider;
        [Tooltip("Slider điều chỉnh âm lượng hiệu ứng")]
        [SerializeField] private Slider sfxVolumeSlider;
        [Tooltip("Button đóng cài đặt")]
        [SerializeField] private Button closeButton;

        private void Start()
        {
            // Ẩn panel cài đặt khi bắt đầu
            settingsPanel.SetActive(false);

            if(musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicChanged);
                musicVolumeSlider.value = SaveManager.LoadMusicVolume();
            }
            if(sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXChanged);
                sfxVolumeSlider.value = SaveManager.LoadSFXVolume();
            }

            if(startButton != null) startButton.onClick.AddListener(OnStartButtonClicked);
            if(settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            if(quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
            if(closeButton != null) closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDestroy()
        {
            if(startButton != null) startButton.onClick.RemoveListener(OnStartButtonClicked);
            if(settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            if(quitButton != null) quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            if(closeButton != null) closeButton.onClick.RemoveListener(OnCloseButtonClicked);

            if(musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicChanged);
            }
            if(sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXChanged);
            }
        }

        private void OnStartButtonClicked()
        {
            SceneManager.LoadScene("LevelSelected");
        }
        private void OnSettingsButtonClicked()
        {
            settingsPanel.SetActive(true);
        }
        private void OnQuitButtonClicked()
        {
            Application.Quit();
        }
        private void OnCloseButtonClicked()
        {
            settingsPanel.SetActive(false);
        }
        private void OnMusicChanged(float value)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        private void OnSFXChanged(float value)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
    }
}
