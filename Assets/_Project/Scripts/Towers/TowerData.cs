using UnityEngine;

namespace TowerDefense.Towers
{
    [System.Serializable]
    public class TowerUpgradeLevel
    {
        [Tooltip("Giá tiền để mua (nếu là cấp 0) hoặc để nâng cấp lên cấp này")]
        public int cost = 50;
        public float damage = 10f;
        public float range = 3f;
        [Tooltip("Số phát bắn trong 1 giây. Vd: 2 = bắn 2 phát 1 giây")]
        public float fireRate = 1f; 
        
        [Header("Slow Effect (Dành riêng cho Slow Tower)")]
        [Tooltip("0.3 = Làm chậm 30%")]
        public float slowPercent = 0.3f; 
        public float slowDuration = 2f;
    }

    [CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data")]
    public class TowerData : ScriptableObject
    {
        public string towerName = "Basic Tower";
        [Tooltip("Đánh dấu true nếu đây là trụ băng/nhớt để đạn có hiệu ứng slow")]
        public bool isSlowTower = false;
        
        [Header("Visual & Audio")]
        public GameObject projectilePrefab;
        public AudioClip shootSound;
        
        [Header("Upgrade Tiers")]
        [Tooltip("Cấp 0 là lúc vừa xây xong, cấp 1 là nâng cấp lần 1...")]
        public TowerUpgradeLevel[] levels;
    }
}
