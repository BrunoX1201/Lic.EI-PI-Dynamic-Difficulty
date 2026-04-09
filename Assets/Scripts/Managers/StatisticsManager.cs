using System;
using Core;
using Unity.FPS.Game;
using UnityEngine;

namespace Managers
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

        public float Accuracy { get; private set; }

        public override void Awake()
        {
            base.Awake();
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
            EventManager.AddListener<GameStartEvent>(OnGameStart);
        }


        private void OnPlayerDeath(PlayerDeathEvent evt)
        {
            TimeAlive.Stop();
            DeathCount++;
            Debug.Log(TimeAlive.EndTime);
            Debug.Log(TimeAlive.DurationInSeconds);
        }

        private void OnGameStart(GameStartEvent evt)
        {
            TimeAlive.Start();
            Debug.Log(TimeAlive.StartTime);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }
    }
}