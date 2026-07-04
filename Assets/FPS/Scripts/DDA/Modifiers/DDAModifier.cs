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

        private readonly T m_minValue;
        private readonly T m_maxValue;
        private readonly Func<T, T, T> m_sum;
        private readonly Func<T, T, T> m_multiply;

        protected DDAModifier(T value, T minValue, T maxValue, Func<T, T, T> sum, Func<T, T, T> multiply)
        {
            m_minValue = minValue;
            m_maxValue = maxValue;
            m_sum = sum;
            m_multiply = multiply;

            Value = Clamp(value);
            OldValue = Value;
        }

        public void Update(T modificationValue, string agent)
        {
            if (!Enum.TryParse<Agent>(agent, true, out Agent result))
                throw new ArgumentException($"Invalid agent value: {agent}.");

            OldValue = Value;

            T newValue = result == Agent.K_means
                ? m_multiply(OldValue, modificationValue)
                : m_sum(OldValue, modificationValue);

            Value = Clamp(newValue);

            UpdateDifficultyDirection();
        }

        private T Clamp(T value)
        {
            if (Comparer<T>.Default.Compare(value, m_minValue) < 0) return m_minValue;
            if (Comparer<T>.Default.Compare(value, m_maxValue) > 0) return m_maxValue;
            return value;
        }

        private void UpdateDifficultyDirection()
        {
            Direction = GetDifficultyDirection(Value, OldValue);
        }

        protected abstract DDADirection GetDifficultyDirection(T currentValue, T previousValue);
    }
}