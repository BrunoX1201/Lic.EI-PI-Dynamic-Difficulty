using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
{
    [RequireComponent(typeof(Health))]
    public class Instigator : MonoBehaviour, ITelemetryInstigator
    {
        public InstigatorType Type { get; }

        public int Id { get; private set; }
        public ITelemetryMapLocation MapLocation { get; private set; }

        public ITelemetryHealth Health { get; private set; }

        private void Awake()
        {
            Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNoComponentFound<Health, Instigator>(1, this, gameObject);
        }

        private void Start()
        {
            InstigatorsManager.Instance.AddInstigator(this);
            Id = InstigatorsManager.Instance.GenerateId();
        }

        public void UpdateLocation(MapLocationSO newLocation)
        {
            MapLocation = newLocation;
        }
    }
}