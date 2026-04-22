using FPS.Scripts.Telemetry.Shared;
using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public readonly struct ItemPickedUpTelemetryData
    {
        public int PlayerId { get; }
        public Item Item { get; }
        public float RemainingAmmo { get; }
        public float RemainingHealth { get; }
        public Vector3 PlayerPosition { get; }

        public ItemPickedUpTelemetryData(int playerId, Item item, float remainingAmmo, float remainingHealth,
            Vector3 playerPosition)
        {
            PlayerId = playerId;
            Item = item;
            RemainingAmmo = remainingAmmo;
            RemainingHealth = remainingHealth;
            PlayerPosition = playerPosition;
        }
    }

    public class ItemPickedUpTelemetry : TelemetryEvent
    {
        public ItemPickedUpTelemetry(string sessionId, ItemPickedUpTelemetryData data) : base(sessionId,
            TelemetryEventType.ItemPickedUp)
        {
            Data.Add("PlayerId", data.PlayerId);
            Data.Add("Item", data.Item);
            Data.Add("RemainingAmmo", data.RemainingAmmo);
            Data.Add("RemainingHealth", data.RemainingHealth);
            Data.Add("PlayerPosition", data.PlayerPosition);
        }
    }
}