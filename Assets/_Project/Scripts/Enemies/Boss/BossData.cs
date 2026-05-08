using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// ScriptableObject chứa cấu hình riêng cho boss.
    /// EnemyData vẫn là nơi giữ stat lõi của enemy; BossData bổ sung
    /// phase, timing cast, audio cue và tuning đặc thù của boss.
    /// </summary>
    [CreateAssetMenu(fileName = "BossData", menuName = "TD/Boss Data", order = 6)]
    public class BossData : ScriptableObject
    {
        [Header("Identity")]
        public string bossName = "New Boss";
        [TextArea(2, 4)] public string description = "";

        [Header("Casting")]
        [Tooltip("Thời gian cast mặc định nếu skill không override.")]
        [Min(0f)] public float defaultCastDuration = 1.5f;

        [Tooltip("Nếu true, boss sẽ đứng yên trong lúc cast skill chủ động.")]
        public bool stopMovementWhileCasting = true;

        [Header("Phase Thresholds")]
        [Tooltip("Danh sách mốc HP chuẩn hóa dùng cho phase hoặc trigger kỹ năng. Ví dụ 0.8, 0.6, 0.5.")]
        public float[] healthThresholds = new float[0];

        [Header("Audio")]
        [Tooltip("Tên cue/BGM/SFX dùng khi boss xuất hiện. Chỉ là dữ liệu tham chiếu.")]
        public string spawnAudioCue = "BossSpawn";

        [Tooltip("Tên cue/BGM/SFX dùng khi boss vào phase nguy hiểm.")]
        public string enragedAudioCue = "BossEnraged";

        [Header("Flags")]
        [Tooltip("Nếu true, boss có thể miễn khống ở một số phase/skill.")]
        public bool supportsCrowdControlImmunity = true;

        /// <summary>
        /// Trả về cast duration hiệu dụng cho một ability.
        /// Nếu ability không có castDuration riêng thì fallback sang defaultCastDuration.
        /// </summary>
        public float GetCastDuration(BossAbility ability)
        {
            if (ability == null)
            {
                return defaultCastDuration;
            }

            return ability.CastDuration > 0f ? ability.CastDuration : defaultCastDuration;
        }
    }
}
