using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Phát nhạc nền và hiệu ứng âm thanh. 
    /// Dùng DontDestroyOnLoad để duy trì nhạc không bị ngắt quãng khi đổi Map.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Common SFX")]
        [Tooltip("Âm thanh mặc định khi bấm nút")]
        [SerializeField] private AudioClip buttonClickSfx;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadVolumeSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadVolumeSettings()
        {
            float bgmVol = PlayerPrefs.GetFloat("BGM_Volume", 1f);
            float sfxVol = PlayerPrefs.GetFloat("SFX_Volume", 1f);

            if (bgmSource != null) bgmSource.volume = bgmVol;
            if (sfxSource != null) sfxSource.volume = sfxVol;
        }

        public void SetBGMVolume(float volume)
        {
            if (bgmSource != null) bgmSource.volume = volume;
            PlayerPrefs.SetFloat("BGM_Volume", volume);
        }

        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null) sfxSource.volume = volume;
            PlayerPrefs.SetFloat("SFX_Volume", volume);
        }

        public float GetBGMVolume() => PlayerPrefs.GetFloat("BGM_Volume", 1f);
        public float GetSFXVolume() => PlayerPrefs.GetFloat("SFX_Volume", 1f);

        public void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null || clip == null) return;
            
            // Tránh phát lại từ đầu nếu cùng một bài nhạc
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Hàm tiện ích: Gắn trực tiếp vào OnClick của bất kỳ Button nào trên UI.
        /// </summary>
        public void PlayButtonClick()
        {
            if (buttonClickSfx != null)
            {
                PlaySFX(buttonClickSfx);
            }
        }
    }
}
