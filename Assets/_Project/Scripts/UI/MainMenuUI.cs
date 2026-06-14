using TowerDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button btnPlayGame;
        [SerializeField] private Button btnSetting;
        [SerializeField] private Button btnQuitGame;
        [SerializeField] private Button btnCloseSetting;

        [Header("Sliders")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        private void Start()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);

            if (AudioManager.Instance != null)
            {
                if (bgmSlider != null)
                {
                    bgmSlider.value = AudioManager.Instance.GetBGMVolume();
                    bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
                }

                if (sfxSlider != null)
                {
                    sfxSlider.value = AudioManager.Instance.GetSFXVolume();
                    sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
                }

                if (btnPlayGame != null)
                {
                    btnPlayGame.onClick.AddListener(PlayGame);
                }

                if (btnSetting != null)
                {
                    btnSetting.onClick.AddListener(OpenSettings);
                }

                if (btnQuitGame != null)
                {
                    btnQuitGame.onClick.AddListener(QuitGame);
                }

                if (btnCloseSetting != null)
                {
                    btnCloseSetting.onClick.AddListener(CloseSettings);
                }
            }
        }

        public void PlayGame()
        {
            AudioManager.Instance.PlayButtonClick();
            SceneManager.LoadScene("LevelSelect");
        }

        public void OpenSettings()
        {
            AudioManager.Instance.PlayButtonClick();
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            AudioManager.Instance.PlayButtonClick();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                PlayerPrefs.Save();
            }
        }

        public void QuitGame()
        {
            AudioManager.Instance.PlayButtonClick();
            Debug.Log("Đã bấm Thoát Game!");
            Application.Quit();
        }

        private void OnBgmVolumeChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
        }
    }
}
