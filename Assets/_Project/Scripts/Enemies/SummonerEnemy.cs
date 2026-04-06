// ============================================================
// SummonerEnemy.cs
// ============================================================
namespace TowerDefense.Enemies
{
    /// <summary>
    /// Enemy triệu hồi — định kỳ spawn thêm minion.
    ///
    /// Cơ chế summon được xử lý bởi SummonAbility component gắn trên Prefab.
    /// SummonerEnemy không cần override gì — SummonAbility tự vận hành.
    public class SummonerEnemy : EnemyBase { }
}