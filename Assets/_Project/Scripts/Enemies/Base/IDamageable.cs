namespace TowerDefense.Enemies
{
    public interface IDamageable
    {
        float CurrentHp { get; }
        float MaxHp { get; }
        float HpPercent { get; }
        bool IsDead { get; }

        void TakeDamage(float damage);
        void Heal(float amount);
    }
}