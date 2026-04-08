using Telemetry.Shared;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapLocationSO", menuName = "Scriptable Objects/MapLocationSO")]
    public class MapLocationSO : ScriptableObject, ITelemetryMapLocation
    {
        [SerializeField] [Range(0, 100)] private int m_GameProgress;

        [SerializeField] private string m_Location;
        public int GameProgress => m_GameProgress;

        public string Location => m_Location;
    }
}