using Unity.FPS.Telemetry;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviour, ITelemetryHealth
    {
        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float CriticalHealthRatio = 0.3f;

        [Tooltip("Maximum amount of health")] [SerializeField]
        private float m_MaxHealth = 10f;

        public float CurrentHealth { get; set; }
        public bool Invincible { get; set; }

        public float MaxHealth => m_MaxHealth;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;
        public UnityAction<GameObject> OnDie;

        private bool m_IsDead;

        private void Start()
        {
            CurrentHealth = m_MaxHealth;
        }

        public bool CanPickup()
        {
            return CurrentHealth < m_MaxHealth;
        }

        public float GetRatio()
        {
            return CurrentHealth / m_MaxHealth;
        }

        public bool IsCritical()
        {
            return GetRatio() <= CriticalHealthRatio;
        }

        public void Heal(float healAmount)
        {
            float healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, m_MaxHealth);

            // call OnHeal action
            float trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f)
            {
                OnHealed?.Invoke(trueHealAmount);
            }
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible)
            {
                return;
            }

            float healthBefore = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, m_MaxHealth);

            // call OnDamage action
            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f)
            {
                OnDamaged?.Invoke(trueDamageAmount, damageSource);
            }

            HandleDeath(damageSource);
        }

        public void Kill()
        {
            CurrentHealth = 0f;

            // call OnDamage action
            OnDamaged?.Invoke(m_MaxHealth, null);

            HandleDeath(null);
        }

        private void HandleDeath(GameObject instigator)
        {
            if (m_IsDead)
            {
                return;
            }

            // call OnDie action
            if (CurrentHealth <= 0f)
            {
                m_IsDead = true;
                OnDie?.Invoke(instigator);
            }
        }
        
        public void SetMaxHealth(float newMax)
        {
            m_MaxHealth = Mathf.Max(0.01f, newMax);
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, m_MaxHealth);
        }
    }
}