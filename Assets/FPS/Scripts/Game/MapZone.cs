using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
{
    [RequireComponent(typeof(Collider))]
    public class MapZone : MonoBehaviour
    {
        [SerializeField] private MapLocationSO m_mapLocationSO;

        public MapLocationSO MapLocation => m_mapLocationSO;

        private void Awake()
        {
            Collider myCollider = GetComponent<Collider>();
            myCollider.isTrigger = true;
        }

        private void Reset()
        {
            gameObject.layer = 13; // MapLocation
            Collider myCollider = GetComponent<Collider>();
            myCollider.isTrigger = true;
        }


        private void OnTriggerEnter(Collider other)
        {
            Instigator instigator = other.gameObject.GetComponent<Instigator>();
            if (instigator == null)
            {
                return;
            }

            bool hasDiscovered = instigator.HasDiscoveredZone(m_mapLocationSO);
            float timeInLastLocation = instigator.LocationEnterTime;

            if (instigator.UpdateLocation(m_mapLocationSO) && instigator.Type == InstigatorType.Player)
            {
                TrackMapZone(instigator, timeInLastLocation, hasDiscovered);
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
                int layerMask = 1 << gameObject.layer;

                Collider[] hitColliders = Physics.OverlapBox(other.transform.position,
                    other.transform.localScale / 2, Quaternion.identity, layerMask);

                foreach (Collider mapZone in hitColliders)
                {
                    if (mapZone.gameObject != gameObject)
                    {
                        return;
                    }
                }

                float timeInLastLocation = instigator.LocationEnterTime;
                instigator.UpdateLocation(null);

                TrackMapZone(instigator, timeInLastLocation, false);
            }
        }

        private void TrackMapZone(Instigator instigator, float timeInLastLocation, bool hasDiscovered)
        {
            int timeInLastLocationSeconds =
                Mathf.Max(0, Mathf.RoundToInt(Time.time - timeInLastLocation));
            NewLocationDiscoveredTelemetryData telemetryData = new(instigator.PreviousMapLocation,
                instigator.MapLocation,
                timeInLastLocationSeconds, hasDiscovered);

            TelemetryService.TrackNewLocationDiscover(SessionManager.Instance.SessionID.ToString(), telemetryData);
        }
    }
}