using System;

namespace Unity.FPS.Game
{
    public class TimeRange
    {
        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }
        public int DurationInSeconds => (int)EndTime.Subtract(StartTime).TotalSeconds;

        public bool IsStopped { get; private set; }

        public void Start()
        {
            StartTime = DateTime.UtcNow;
            EndTime = DateTime.MinValue;
            IsStopped = false;
        }

        public void Stop()
        {
            if (StartTime == DateTime.MinValue || IsStopped)
            {
                return;
            }

            EndTime = DateTime.UtcNow;
            IsStopped = true;
        }

        public void Continue()
        {
            IsStopped = false;
        }
    }

    public class StatisticsManager : Singleton<StatisticsManager>
    {
        public int DeathCount { get; private set; }

        public TimeRange TimeAlive { get; } = new();

        public float Accuracy => m_totalPalletsFired > 0 ? (float)m_totalPalletsHit / m_totalPalletsFired : 0f;

        public TimeRange TotalGameTime { get; } = new();

        private uint m_totalPalletsFired;
        private uint m_totalPalletsHit;
        private uint m_totalPalletsMissed;

        public void IncrementDeathCount(int increment)
        {
            DeathCount += increment;
        }

        public void UpdateAccuracy(uint palletsFired, bool areHits)
        {
            m_totalPalletsFired += palletsFired;
            if (areHits)
            {
                m_totalPalletsHit += palletsFired;
            }
            else
            {
                m_totalPalletsMissed += palletsFired;
            }
        }
    }
}