using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
{
    [RequireComponent(typeof(Health))]
    public class Instigator : MonoBehaviour, ITelemetryInstigator
    {
        [SerializeField] private InstigatorType m_instigatorType;

        public InstigatorType Type => m_instigatorType;

        public int Id { get; private set; }
        public ITelemetryMapLocation MapLocation => m_mapLocation;

        public ITelemetryHealth Health { get; private set; }
        private MapLocationSO m_mapLocation;

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
            m_mapLocation = newLocation;
        }
    }
}