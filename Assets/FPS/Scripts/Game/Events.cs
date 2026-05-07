using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
{
    // The Game Events used across the Game.
    // Anytime there is a need for a new event, it should be added here.

    public static class Events
    {
        public static ObjectiveUpdateEvent ObjectiveUpdateEvent = new();
        public static AllObjectivesCompletedEvent AllObjectivesCompletedEvent = new();
        public static GameOverEvent GameOverEvent = new();
        public static PlayerDeathEvent PlayerDeathEvent = new();
        public static EnemyKillEvent EnemyKillEvent = new();
        public static PickupEvent PickupEvent = new();
        public static AmmoPickupEvent AmmoPickupEvent = new();
        public static DamageEvent DamageEvent = new();
        public static DisplayMessageEvent DisplayMessageEvent = new();
        public static ObstacleUnblockEvent ObstacleUnblockEvent = new();
    }

    public class ObjectiveUpdateEvent : GameEvent
    {
        public Objective Objective;
        public string DescriptionText;
        public string CounterText;
        public bool IsComplete;
        public string NotificationText;
    }

    public class AllObjectivesCompletedEvent : GameEvent
    {
    }

    public class GameOverEvent : GameEvent
    {
        public bool Win;
    }

    public class PlayerDeathEvent : GameEvent
    {
        public ITelemetryInstigator Instigator;
    }

    public class EnemyKillEvent : GameEvent
    {
        public GameObject Enemy;
        public int RemainingEnemyCount;
    }

    public class PickupEvent : GameEvent
    {
        public GameObject Pickup;
    }

    public class AmmoPickupEvent : GameEvent
    {
        public WeaponController Weapon;
    }

    public class DamageEvent : GameEvent
    {
        public GameObject Sender;
        public float DamageValue;
    }

    public class DisplayMessageEvent : GameEvent
    {
        public string Message;
        public float DelayBeforeDisplay;
    }

    public class ObstacleUnblockEvent : GameEvent
    {
        public string ObstacleId;
    }
}