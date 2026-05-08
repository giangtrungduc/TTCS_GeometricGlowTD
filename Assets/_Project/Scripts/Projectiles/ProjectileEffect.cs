using UnityEngine;

namespace TowerDefense.Projectiles
{
    public abstract class ProjectileEffect : ScriptableObject
    {
        public abstract void Apply(GameObject target);
    }
}