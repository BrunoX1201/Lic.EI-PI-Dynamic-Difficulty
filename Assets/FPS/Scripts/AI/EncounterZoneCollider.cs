using Unity.FPS.Game;
using Unity.FPS.Telemetry;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    public class EncounterZoneCollider : MonoBehaviour
    {
        public const int Layer = 14; // EncounterZone
        public UnityAction<Instigator, GameObject> OnPlayerEntered;
        public UnityAction<Instigator, GameObject> OnPlayerExited;

        private void Awake()
        {
            gameObject.layer = Layer;
        }

        private void OnTriggerEnter(Collider other)
        {
            Instigator instigator = other.GetComponent<Instigator>();
            if (instigator == null || instigator.Type != InstigatorType.Player)
            {
                return;
            }

            OnPlayerEntered?.Invoke(instigator, gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            Instigator instigator = other.GetComponent<Instigator>();
            if (instigator == null || instigator.Type != InstigatorType.Player)
            {
                return;
            }

            OnPlayerExited?.Invoke(instigator, gameObject);
        }
    }
}