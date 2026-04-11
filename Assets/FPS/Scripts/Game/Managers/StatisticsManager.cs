using System;

namespace Unity.FPS.Game
{
    public class TimeRange
    {
        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public int DurationInSeconds => (int)EndTime.Subtract(StartTime).TotalSeconds;

        public void Start()
        {
            StartTime = DateTime.Now;
            EndTime = DateTime.MinValue;
        }

        public void Stop()
        {
            if (StartTime == DateTime.MinValue)
            {
                return;
            }

            EndTime = DateTime.Now;
        }
    }

    public class StatisticsManager : Singleton<StatisticsManager>
    {
        public int DeathCount { get; private set; }

        public TimeRange TimeAlive { get; } = new();

        public float Accuracy => m_totalPalletsFired > 0 ? (float)m_totalPalletsHit / m_totalPalletsFired : 0f;

        private uint m_totalPalletsFired;
        private uint m_totalPalletsHit;
        private uint m_totalPalletsMissed;

        public void StartAliveTimer()
        {
            TimeAlive.Start();
        }

        public void StopAliveTimer()
        {
            TimeAlive.Stop();
        }

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