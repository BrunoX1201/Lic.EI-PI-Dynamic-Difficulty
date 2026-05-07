using System;
using PrimeTween;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class DoorController : MonoBehaviour
    {
        [Serializable]
        private class AnimationConfiguration
        {
            public Vector3 OpenEndOffset;
        }

        [SerializeField] private Door m_door;
        [SerializeField] private AnimationConfiguration m_animationConfiguration;

        public bool IsOpened { get; private set; }

        private void Start()
        {
            m_door.OnUnblocked += Open;
        }

        private void OnDestroy()
        {
            m_door.OnUnblocked -= Open;
        }

        public void Open()
        {
            if (IsOpened)
            {
                return;
            }

            Vector3 endPosition = m_door.transform.position + m_animationConfiguration.OpenEndOffset;
            Tween.Position(m_door.transform,
                new TweenSettings<Vector3>(endPosition,
                    new TweenSettings(2, Ease.OutExpo)));

            IsOpened = true;
        }

        public void Close()
        {
            if (!IsOpened)
            {
                return;
            }

            Vector3 endPosition = m_door.transform.position - m_animationConfiguration.OpenEndOffset;
            Tween.Position(m_door.transform,
                new TweenSettings<Vector3>(endPosition, new TweenSettings(2, Ease.OutExpo)));

            IsOpened = false;
        }
    }
}