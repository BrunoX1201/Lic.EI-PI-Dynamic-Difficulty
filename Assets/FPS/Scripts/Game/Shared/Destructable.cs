using UnityEngine;

namespace Unity.FPS.Game
{
    public class Destructable : MonoBehaviour
    {
        private Health m_Health;

        private void Start()
        {
            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(m_Health, this, gameObject);

            // Subscribe to damage & death actions
            m_Health.OnDie += OnDie;
            m_Health.OnDamaged += OnDamaged;
        }

        private void OnDamaged(float damage, GameObject damageSource)
        {
            // TODO: damage reaction
        }

        private void OnDie(GameObject instigator)
        {
            // this will call the OnDestroy function
            Destroy(gameObject);
        }
    }
}