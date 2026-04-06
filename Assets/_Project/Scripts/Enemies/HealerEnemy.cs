namespace TowerDefense.Enemies
{
    /// <summary>
    /// Enemy hỗ trợ — hồi máu cho enemy lân cận.
    ///
    /// Cơ chế heal được xử lý bởi AuraAbility component gắn trên Prefab.
    /// HealerEnemy không cần override gì — AuraAbility tự vận hành qua InvokeRepeating.
    /// </summary>
    public class HealerEnemy : EnemyBase { }
}