using System.Collections.Generic;
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
        public MapLocationSO PreviousMapLocation { get; private set; }

        public float LocationEnterTime { get; private set; }

        public ITelemetryHealth Health { get; private set; }
        private readonly HashSet<MapLocationSO> m_discoveredZones = new();
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
            if (m_mapLocation == newLocation)
            {
                return;
            }

            if (m_mapLocation != null)
            {
                PreviousMapLocation = m_mapLocation;
            }

            m_mapLocation = newLocation;
            MarkZoneDiscovered(newLocation);

            LocationEnterTime = Time.time;
        }

        public bool HasDiscoveredZone(MapLocationSO zone)
        {
            return zone != null && m_discoveredZones.Contains(zone);
        }

        private void MarkZoneDiscovered(MapLocationSO zone)
        {
            if (zone == null || HasDiscoveredZone(zone))
            {
                return;
            }

            m_discoveredZones.Add(zone);
        }
    }
}