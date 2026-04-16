using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class MapZone : MonoBehaviour
    {
        [SerializeField] private MapLocationSO m_mapLocationSO;

        public MapLocationSO MapLocation => m_mapLocationSO;

        private void OnTriggerEnter(Collider other)
        {
            Instigator instigator = other.gameObject.GetComponent<Instigator>();
            if (instigator == null)
            {
                return;
            }

            bool hasDiscovered = instigator.HasDiscoveredZone(m_mapLocationSO);
            instigator.UpdateLocation(m_mapLocationSO);

            if (instigator.Type == InstigatorType.Player)
            {
                int timeInLastLocationSeconds =
                    Mathf.Max(0, Mathf.RoundToInt(Time.time - instigator.LocationEnterTime));
                NewLocationDiscoveredTelemetryData telemetryData = new(instigator.PreviousMapLocation,
                    instigator.MapLocation,
                    timeInLastLocationSeconds, hasDiscovered);

                TelemetryService.TrackNewLocationDiscover(Constants.DefaultSessionId, telemetryData);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Instigator instigator = other.gameObject.GetComponent<Instigator>();
            if (instigator == null)
            {
                return;
            }

            if (instigator.MapLocation as MapLocationSO == m_mapLocationSO)
            {
                instigator.UpdateLocation(null);
            }
        }
    }
}