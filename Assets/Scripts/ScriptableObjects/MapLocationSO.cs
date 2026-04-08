using Telemetry.Shared;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MapLocationSO", menuName = "Scriptable Objects/MapLocationSO")]
    public class MapLocationSO : ScriptableObject, ITelemetryMapLocation
    {
        [SerializeField] [Range(0, 100)] private int m_gameProgress;
        [SerializeField] private string m_location;

        public int GameProgress => m_gameProgress;
        public string Location => m_location;
    }
}