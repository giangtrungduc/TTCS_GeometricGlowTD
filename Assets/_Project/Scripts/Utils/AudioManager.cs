using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Utils
{
    // ===========================
    // SFX TYPE ENUM
    // ===========================

    /// <summary>
    /// Định danh các loại SFX.
    /// </summary>
    public enum SFXType
    {
        Shoot,
        Hit,
        Place,
        Die,
        Upgrade,
        Sell
    }

    // ===========================
    // SFX ENTRY
    // ===========================

    [System.Serializable]
    public class SFXEntry
    {
        [Tooltip("Loại SFX")]
        public SFXType type;

        [Tooltip("Audio clip tương ứng")]
        public AudioClip clip;

        [Tooltip("Volume riêng. -1 = dùng defaultVolume của Manager.")]
        [Range(-1f, 1f)]
        public float volumeOverride = -1f;
    }

    // ===========================
    // AUDIO MANAGER
    // ===========================

    public class AudioManager : ManagerBase<AudioManager>
    {
        // ===========================
        // CẤU HÌNH
        // ===========================

        [Header("SFX Entries")]
        [Tooltip("Kéo clip vào đây. Mỗi entry = 1 loại SFX + volume riêng tuỳ chọn.")]
        [SerializeField] private SFXEntry[] sfxEntries;

        [Header("Pool Settings")]
        [Tooltip("Số AudioSource pool. Tăng nếu nhiều SFX phát đồng thời.")]
        [SerializeField][Range(3, 16)] private int poolSize = 6;

        [Tooltip("Volume mặc định cho SFX.")]
        [SerializeField][Range(0f, 1f)] private float defaultVolume = 0.85f;

        // ===========================
        // STATE
        // ===========================

        private AudioSource[] sources;
        private readonly Dictionary<SFXType, SFXEntry> sfxMap = new Dictionary<SFXType, SFXEntry>();

        // ===========================
        // LIFECYCLE
        // ===========================

        protected override void OnAwake()
        {
            base.OnAwake();
            CreateSourcePool();
            BuildSFXMap();
        }

        private void OnEnable()
        {
            GameEvents.OnTowerPlaced += HandleTowerPlaced;
            GameEvents.OnTowerUpgraded += HandleTowerUpgraded;
            GameEvents.OnTowerSold += HandleTowerSold;
            GameEvents.OnEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            GameEvents.OnTowerPlaced -= HandleTowerPlaced;
            GameEvents.OnTowerUpgraded -= HandleTowerUpgraded;
            GameEvents.OnTowerSold -= HandleTowerSold;
            GameEvents.OnEnemyDied -= HandleEnemyDied;
        }

        // ===========================
        // POOL SETUP
        // ===========================

        private void CreateSourcePool()
        {
            sources = new AudioSource[poolSize];

            GameObject poolRoot = new GameObject("SFX_Pool");
            poolRoot.transform.SetParent(transform);

            for (int i = 0; i < poolSize; i++)
            {
                GameObject go = new GameObject($"SFX_Source_{i}");
                go.transform.SetParent(poolRoot.transform);

                AudioSource src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = defaultVolume;

                sources[i] = src;
            }
        }

        private void BuildSFXMap()
        {
            if (sfxEntries == null) return;

            foreach (SFXEntry entry in sfxEntries)
            {
                if (entry == null || entry.clip == null) continue;

                if (sfxMap.ContainsKey(entry.type))
                {
                    Debug.LogWarning($"[AudioManager] SFXType '{entry.type}' bị khai báo trùng — giữ entry đầu tiên.", this);
                    continue;
                }

                sfxMap[entry.type] = entry;
            }
        }

        // ===========================
        // PUBLIC API
        // ===========================

        /// <summary>
        /// Phát SFX theo loại.
        /// </summary>
        public void PlaySFX(SFXType type)
        {
            if (!sfxMap.TryGetValue(type, out SFXEntry entry))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[AudioManager] SFXType '{type}' chưa được khai báo trong sfxEntries.", this);
#endif
                return;
            }

            AudioSource src = GetAvailableSource();

            // Áp dụng volume override nếu có
            float vol = entry.volumeOverride >= 0f ? entry.volumeOverride : defaultVolume;
            src.PlayOneShot(entry.clip, vol);
        }

        // ===========================
        // POOL — TÌM SOURCE RẢNH
        // ===========================

        /// <summary>
        /// Tìm AudioSource không đang phát.
        /// </summary>
        private AudioSource GetAvailableSource()
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if (!sources[i].isPlaying)
                    return sources[i];
            }
            return sources[0];
        }

        // ===========================
        // EVENT HANDLERS
        // ===========================

        private void HandleTowerPlaced(GameObject _) => PlaySFX(SFXType.Place);
        private void HandleTowerUpgraded(GameObject _) => PlaySFX(SFXType.Upgrade);
        private void HandleTowerSold(GameObject _) => PlaySFX(SFXType.Sell);
        private void HandleEnemyDied(GameObject _) => PlaySFX(SFXType.Die);

        // ===========================
        // SHORTHAND API
        // (dùng khi Tower/Projectile muốn phát trực tiếp)
        // ===========================

        public void PlayShoot() => PlaySFX(SFXType.Shoot);
        public void PlayHit() => PlaySFX(SFXType.Hit);

        // ===========================
        // DEBUG
        // ===========================
#if UNITY_EDITOR
        /// <summary>In trạng thái pool — bao nhiêu source đang phát.</summary>
        [ContextMenu("Log Pool Status")]
        private void LogPoolStatus()
        {
            int active = 0;
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i].isPlaying) active++;
            }
            Debug.Log($"[AudioManager] Pool: {active}/{poolSize} sources đang phát.");
        }
#endif
    }
}