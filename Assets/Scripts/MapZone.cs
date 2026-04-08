using ScriptableObjects;
using UnityEngine;

public class MapZone : MonoBehaviour
{
    [SerializeField] private MapLocationSO m_mapLocationSO;


    private void OnTriggerStay(Collider other)
    {
        Instigator instigator = other.gameObject.GetComponent<Instigator>();
        if (instigator == null) return;

        instigator.UpdateLocation(m_mapLocationSO);
    }

    private void OnTriggerExit(Collider other)
    {
        Instigator instigator = other.gameObject.GetComponent<Instigator>();
        if (instigator == null) return;

        instigator.UpdateLocation(null);
    }
}