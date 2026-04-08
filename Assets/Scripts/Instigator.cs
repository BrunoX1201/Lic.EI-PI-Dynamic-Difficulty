using Managers;
using ScriptableObjects;
using Telemetry.Shared;
using Telemetry.Shared.Instigator;
using Unity.FPS.Game;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Instigator : MonoBehaviour, ITelemetryInstigator
{
    [SerializeField] private InstigatorType m_instigatorType;

    public InstigatorType Type => m_instigatorType;
    public int Id { get; private set; }
    public ITelemetryMapLocation MapLocation => m_mapLocationSO;
    public ITelemetryHealth Health => m_health;

    private MapLocationSO m_mapLocationSO;
    private Health m_health;

    private void Awake()
    {
        m_health = GetComponent<Health>();
        DebugUtility.HandleErrorIfNoComponentFound<Health, Instigator>(1, this, gameObject);
    }

    private void Start()
    {
        InstigatorsManager.Instance.AddInstigator(this);
        Id = InstigatorsManager.Instance.GenerateId();
    }

    public void UpdateLocation(MapLocationSO newLocation)
    {
        m_mapLocationSO = newLocation;
    }
}