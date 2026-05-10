using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public abstract class Obstacle : MonoBehaviour
    {
        [SerializeField] private string m_id;

        public string Id => m_id;
        public bool IsBlocking { get; private set; } = true;

        public UnityAction OnUnblocked;

        protected virtual void Start()
        {
            EventManager.AddListener<ObstacleUnblockEvent>(OnObstacleUnblock);
        }

        protected virtual void OnDestroy()
        {
            EventManager.RemoveListener<ObstacleUnblockEvent>(OnObstacleUnblock);
        }

        protected virtual void Unblock()
        {
            IsBlocking = false;
        }

        private void OnObstacleUnblock(ObstacleUnblockEvent evt)
        {
            if (!evt.ObstacleId.Equals(m_id))
            {
                return;
            }

            Unblock();
            OnUnblocked?.Invoke();
        }
    }
}