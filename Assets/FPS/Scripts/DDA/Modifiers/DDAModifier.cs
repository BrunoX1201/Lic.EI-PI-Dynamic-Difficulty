using System;
using System.Collections.Generic;

namespace Unity.FPS.DDA
{
    public enum DDADirection
    {
        MuchEasier = -2,
        Easier = -1,
        Same = 0,
        Harder = 1,
        MuchHarder = 2
    }

    public enum Agent
    {
        K_means,
        Q_learning
    }

    public abstract class DDAModifier<T> : IDDAModifier
    {
        public T Value { get; private set; }
        public T OldValue { get; private set; }
        public DDADirection Direction { get; private set; }

        private readonly Func<T, T, T> m_sum;

        protected DDAModifier(T value, Func<T, T, T> sum)
        {
            Value = value;
            OldValue = value;
            m_sum = sum;
        }

        public void Update(T modificationValue)
        {
            OldValue = Value;
            Value = m_sum(OldValue, modificationValue);
            UpdateDifficultyDirection();
        }

        private void UpdateDifficultyDirection()
        {
            Direction = GetDifficultyDirection(Value, OldValue);
        }

        protected abstract DDADirection GetDifficultyDirection(T currentValue, T previousValue);
    }
}