using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace TowerDefense.Core
{
    public class AudioManager : ManagerBase<AudioManager>
    {
        protected override bool Persistent => true;

        [System.Serializable]
        public class SceneBgmEntry
        {
            public string sceneName;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            public bool loop = true;
        }

        public enum SfxEvent
        {
            EnemyDied,
            EnemyReachedEnd,
            TowerPlaced,
            TowerUpgraded,
            TowerSold,
            LevelWin,
            LevelLose
        }

        [System.Serializable]
        public class EventSfxEntry
        {
            public SfxEvent sfxEvent;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [Header("BGM By Scene")]
        [SerializeField] private List<SceneBgmEntry> sceneBgms = new();

        [Header("SFX By Game Event")]
        [SerializeField] private List<EventSfxEntry> eventSfx = new();

        [Header("Mixer")]
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField, Min(0f)] private float bgmFadeTime = 0.3f;

        private readonly Dictionary<string, SceneBgmEntry> bgmByScene = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<SfxEvent, EventSfxEntry> sfxByEvent = new();

        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private Coroutine bgmFadeRoutine;
        private SceneBgmEntry currentBgmEntry;

        private float musicVolume = 1f;
        private float sfxVolume = 1f;
        private float masterVolume = 1f;

        protected override void OnAwake()
        {
            BuildLookups();
            CreateAudioSources();
            LoadVolumes();
            SubscribeEvents();
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeEvents();
        }

        private void BuildLookups()
        {
            bgmByScene.Clear();
            sfxByEvent.Clear();

            foreach (var entry in sceneBgms)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.sceneName) || entry.clip == null)
                {
                    continue;
                }

                bgmByScene[entry.sceneName.Trim()] = entry;
            }

            foreach (var entry in eventSfx)
            {
                if (entry == null || entry.clip == null)
                {
                    continue;
                }

                sfxByEvent[entry.sfxEvent] = entry;
            }
        }

        private void CreateAudioSources()
        {
            bgmSource = CreateSource("BGM_Source", musicGroup);
            bgmSource.loop = true;

            sfxSource = CreateSource("SFX_Source", sfxGroup);
        }

        private AudioSource CreateSource(string name, AudioMixerGroup group)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = group;

            return source;
        }

        private void LoadVolumes()
        {
            musicVolume = SaveManager.LoadMusicVolume();
            sfxVolume = SaveManager.LoadSFXVolume();
            masterVolume = 1f;

            ApplyVolumes();
        }

        private void SubscribeEvents()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            GameEvents.OnEnemyDied += HandleEnemyDied;
            GameEvents.OnEnemyReachedEnd += HandleEnemyReachedEnd;
            GameEvents.OnTowerPlaced += HandleTowerPlaced;
            GameEvents.OnTowerUpgraded += HandleTowerUpgraded;
            GameEvents.OnTowerSold += HandleTowerSold;
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
        }

        private void UnsubscribeEvents()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            GameEvents.OnEnemyDied -= HandleEnemyDied;
            GameEvents.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
            GameEvents.OnTowerPlaced -= HandleTowerPlaced;
            GameEvents.OnTowerUpgraded -= HandleTowerUpgraded;
            GameEvents.OnTowerSold -= HandleTowerSold;
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!bgmByScene.TryGetValue(scene.name, out var entry))
            {
                return;
            }

            if (currentBgmEntry != null && currentBgmEntry.clip == entry.clip && bgmSource.isPlaying)
            {
                return;
            }

            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
            }

            bgmFadeRoutine = StartCoroutine(SwitchBgmWithFade(entry));
        }

        private IEnumerator SwitchBgmWithFade(SceneBgmEntry next)
        {
            float fade = Mathf.Max(0f, bgmFadeTime);
            float fadeOutStart = bgmSource.volume;

            if (fade > 0f && bgmSource.isPlaying)
            {
                float t = 0f;
                while (t < fade)
                {
                    t += Time.unscaledDeltaTime;
                    bgmSource.volume = Mathf.Lerp(fadeOutStart, 0f, t / fade);
                    yield return null;
                }
            }

            bgmSource.Stop();
            bgmSource.clip = next.clip;
            bgmSource.loop = next.loop;
            currentBgmEntry = next;
            bgmSource.volume = 0f;
            bgmSource.Play();

            float target = next.volume * musicVolume * masterVolume;
            if (fade <= 0f)
            {
                bgmSource.volume = target;
                bgmFadeRoutine = null;
                yield break;
            }

            float tIn = 0f;
            while (tIn < fade)
            {
                tIn += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, target, tIn / fade);
                yield return null;
            }

            bgmSource.volume = target;
            bgmFadeRoutine = null;
        }

        private void HandleEnemyDied(GameObject _)
        {
            PlayEventSfx(SfxEvent.EnemyDied);
        }

        private void HandleEnemyReachedEnd(GameObject _)
        {
            PlayEventSfx(SfxEvent.EnemyReachedEnd);
        }

        private void HandleTowerPlaced(GameObject _)
        {
            PlayEventSfx(SfxEvent.TowerPlaced);
        }

        private void HandleTowerUpgraded(GameObject _)
        {
            PlayEventSfx(SfxEvent.TowerUpgraded);
        }

        private void HandleTowerSold(GameObject _)
        {
            PlayEventSfx(SfxEvent.TowerSold);
        }

        private void HandleLevelCompleted(LevelResult result)
        {
            PlayEventSfx(result != null && result.isVictory ? SfxEvent.LevelWin : SfxEvent.LevelLose);
        }

        public void PlayEventSfx(SfxEvent sfxEvent)
        {
            if (!sfxByEvent.TryGetValue(sfxEvent, out var entry) || entry.clip == null)
            {
                return;
            }

            float volume = entry.volume * sfxVolume * masterVolume;
            sfxSource.PlayOneShot(entry.clip, volume);
        }

        public void StopBgm()
        {
            if (bgmFadeRoutine != null)
            {
                StopCoroutine(bgmFadeRoutine);
                bgmFadeRoutine = null;
            }
            bgmSource.Stop();
            currentBgmEntry = null;
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            SaveManager.SaveMusicVolume(musicVolume);
            ApplyVolumes();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            SaveManager.SaveSFXVolume(sfxVolume);
            ApplyVolumes();
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null && currentBgmEntry != null)
            {
                bgmSource.volume = currentBgmEntry.volume * musicVolume * masterVolume;
            }
        }
    }
}
