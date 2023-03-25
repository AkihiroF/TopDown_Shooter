using UnityEngine;

namespace _Source.HealthSystem
{
    public abstract class ABaseHealth : MonoBehaviour , IHealthObject
    {
        protected float CurrentHp;
        [SerializeField] protected float maxHp;

        protected  virtual void Start()
        {
            CurrentHp = maxHp;
        }

        public abstract void GetDamage(float damage);

        public abstract void ReturnHealth(float health);
    }
}