using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Game
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