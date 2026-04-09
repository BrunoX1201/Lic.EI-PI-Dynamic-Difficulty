using System.Threading.Tasks;
using Telemetry.Events;
using Telemetry.Shared;
using Telemetry.Uploaders;
using UnityEngine;

namespace Telemetry
{
    public static class TelemetryService
    {
        private static TelemetryBatcher _batcher;

        public static void Initialize(ITelemetryUploader uploader = null, int batchSize = 10)
        {
            _batcher = new TelemetryBatcher(batchSize, uploader);
        }

        public static async ValueTask ShutdownAsync()
        {
            await _batcher.DisposeAsync();
            _batcher = null;
        }

        public static void Shutdown()
        {
            _batcher.Dispose();
            _batcher = null;
        }

        public static void TrackPlayerAttack(int sessionId, int playerId, int targetId, bool isHit, float accuracy,
            Vector3 playerPosition,
            Weapon weaponUsed, float damagePerHit, AttackType attackType, float remainingAmmo)
        {
            PlayerAttackedTelemetry telemetryEvent = new(
                sessionId,
                playerId,
                targetId,
                isHit,
                accuracy,
                playerPosition,
                weaponUsed,
                damagePerHit,
                attackType,
                remainingAmmo);

            TelemetryEventBus.Publish(telemetryEvent);
        }
    }
}