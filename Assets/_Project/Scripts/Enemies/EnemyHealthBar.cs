using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Hiển thị thanh máu trên đầu Enemy. Tách biệt hoàn toàn khỏi logic của EnemyBase.
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [Tooltip("Kéo script EnemyBase vào đây")]
        [SerializeField] private EnemyBase enemy;
        
        [Tooltip("Hình chữ nhật dẹt làm thanh máu, Pivot ở Left")]
        [SerializeField] private Transform fillTransform;
        
        [Tooltip("Object cha chứa toàn bộ UI thanh máu (để ẩn hiện)")]
        [SerializeField] private GameObject barContainer;

        private SpriteRenderer fillSpriteRenderer;

        private void Awake()
        {
            if (fillTransform != null)
            {
                fillSpriteRenderer = fillTransform.GetComponent<SpriteRenderer>();
            }
        }

        private void LateUpdate()
        {
            if (enemy == null || fillTransform == null || barContainer == null) return;

            float hpPct = enemy.HpPercent;

            // Luôn hiện thanh máu khi quái còn sống
            bool shouldShow = hpPct > 0f && !enemy.IsDead;
            
            if (barContainer.activeSelf != shouldShow)
            {
                barContainer.SetActive(shouldShow);
            }

            if (shouldShow)
            {
                // Thu phóng trục X theo % máu
                fillTransform.localScale = new Vector3(hpPct, 0.15f, 1f);
                
                // Đổi màu từ Đỏ (0) -> Cam -> Vàng -> Xanh lá (0.33)
                if (fillSpriteRenderer != null)
                {
                    fillSpriteRenderer.color = Color.HSVToRGB(hpPct / 3f, 1f, 1f);
                }
                
                transform.rotation = Quaternion.identity;
            }
        }
    }
}
