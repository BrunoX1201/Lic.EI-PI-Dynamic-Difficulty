using Unity.FPS.Telemetry;

namespace Unity.FPS.Game
{
    public class ConstantInstigator : ITelemetryInstigator
    {
        public InstigatorType Type { get; }
        public int Id { get; }
        public ITelemetryMapLocation MapLocation { get; }
        public ITelemetryHealth Health { get; }

        public ConstantInstigator(InstigatorType type, int id, ITelemetryMapLocation mapLocation,
            ITelemetryHealth health)
        {
            Type = type;
            Id = id;
            MapLocation = mapLocation;
            Health = health;
        }
    }

    public static class TelemetryConstants
    {
        public static readonly ConstantInstigator SystemInstigator =
            new(InstigatorType.System, (int)SpecialId.System, null, null);
    }
}