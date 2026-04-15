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
        public float LocationEnterTime => m_locationEnterTime;

        public ITelemetryHealth Health { get; private set; }
        private MapLocationSO m_mapLocation;
        private float m_locationEnterTime;
        private readonly HashSet<MapLocationSO> m_discoveredZones = new();

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

            m_mapLocation = newLocation;
            m_locationEnterTime = Time.time;
        }

        public bool HasDiscoveredZone(MapLocationSO zone)
        {
            return zone != null && m_discoveredZones.Contains(zone);
        }

        public void MarkZoneDiscovered(MapLocationSO zone)
        {
            if (zone == null)
            {
                return;
            }

            m_discoveredZones.Add(zone);
        }
    }
}