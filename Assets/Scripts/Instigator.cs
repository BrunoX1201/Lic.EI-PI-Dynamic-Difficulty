using Managers;
using ScriptableObjects;
using Telemetry.Shared;
using Telemetry.Shared.Instigator;
using Unity.FPS.Game;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Instigator : MonoBehaviour, ITelemetryInstigator
{
    [SerializeField] private InstigatorType m_InstigatorType;

    public InstigatorType Type => m_InstigatorType;
    public int Id { get; private set; }
    public ITelemetryMapLocation MapLocation => m_MapLocationSO;
    public ITelemetryHealth Health => m_Health;

    private MapLocationSO m_MapLocationSO;
    private Health m_Health;

    private void Awake()
    {
        m_Health = GetComponent<Health>();
        DebugUtility.HandleErrorIfNoComponentFound<Health, Instigator>(1, this, gameObject);
    }

    private void Start()
    {
        InstigatorsManager.Instance.AddInstigator(this);
        Id = InstigatorsManager.Instance.GenerateId();
    }

    public void UpdateLocation(MapLocationSO newLocation)
    {
        m_MapLocationSO = newLocation;
    }
}