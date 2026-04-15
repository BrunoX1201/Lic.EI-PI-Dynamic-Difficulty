using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class MapZone : MonoBehaviour
    {
        [SerializeField] private MapLocationSO m_mapLocationSO;

        private void OnTriggerEnter(Collider other)
        {
            TryProcessZone(other, true);
        }

        private void OnTriggerStay(Collider other)
        {
            // Stay is used only to initialize location when player starts inside a trigger.
            TryProcessZone(other, false);
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

        private void TryProcessZone(Collider other, bool allowTelemetryPublish)
        {
            Instigator instigator = other.gameObject.GetComponent<Instigator>();
            if (instigator == null || m_mapLocationSO == null)
            {
                return;
            }

            MapLocationSO previousLocation = instigator.MapLocation as MapLocationSO;
            if (previousLocation == m_mapLocationSO)
            {
                // Instigator can start already inside this zone; keep discovered state in sync.
                instigator.MarkZoneDiscovered(m_mapLocationSO);
                return;
            }

            // First valid zone assignment is initialization, not a room-change transition.
            if (previousLocation == null)
            {
                instigator.UpdateLocation(m_mapLocationSO);
                instigator.MarkZoneDiscovered(m_mapLocationSO);
                return;
            }

            if (!allowTelemetryPublish)
            {
                return;
            }

            bool hasDiscovered = instigator.HasDiscoveredZone(m_mapLocationSO);
            if (instigator.Type == InstigatorType.Player)
            {
                int timeInLastLocationSeconds =
                    Mathf.Max(0, Mathf.RoundToInt(Time.time - instigator.LocationEnterTime));
                NewLocationDiscoveredTelemetryData telemetryData = new(previousLocation, m_mapLocationSO,
                    timeInLastLocationSeconds, hasDiscovered);

                TelemetryService.TrackNewLocationDiscover(Constants.DefaultSessionId, telemetryData);
            }

            instigator.UpdateLocation(m_mapLocationSO);
            instigator.MarkZoneDiscovered(m_mapLocationSO);
        }
    }
}