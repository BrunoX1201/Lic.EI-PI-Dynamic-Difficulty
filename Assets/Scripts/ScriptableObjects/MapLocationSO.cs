using Telemetry.Shared;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapLocation", menuName = "Scriptable Objects/MapLocation")]
    public class MapLocationSO : ScriptableObject, ITelemetryMapLocation
    {
        [SerializeField] private int m_GameProgress;

        [SerializeField] private string m_Location;

        public int GameProgress => m_GameProgress;

        public string Location => m_Location;
    }
}