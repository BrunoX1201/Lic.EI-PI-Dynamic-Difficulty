using Unity.FPS.Game;
using Unity.FPS.Telemetry;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public enum PickupType
    {
        Health = 0,
        Ammo = 1,
        Weapon = 2,
        Jetpack = 3
    }

    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        [Tooltip("Frequency at which the item will move up and down")]
        public float VerticalBobFrequency = 1f;

        [Tooltip("Distance the item will move up and down")]
        public float BobbingAmount = 1f;

        [Tooltip("Rotation angle per second")] public float RotatingSpeed = 360f;

        [Tooltip("Sound played on pickup")] public AudioClip PickupSfx;
        [Tooltip("VFX spawned on pickup")] public GameObject PickupVfxPrefab;

        public Rigidbody PickupRigidbody { get; private set; }

        public PickupType Type => m_type;

        protected PickupType m_type;

        private Collider m_Collider;
        private Vector3 m_StartPosition;
        private bool m_HasPlayedFeedback;


        protected virtual void Start()
        {
            PickupRigidbody = GetComponent<Rigidbody>();
            DebugUtility.HandleErrorIfNullGetComponent<Rigidbody, Pickup>(PickupRigidbody, this, gameObject);
            m_Collider = GetComponent<Collider>();
            DebugUtility.HandleErrorIfNullGetComponent<Collider, Pickup>(m_Collider, this, gameObject);

            // ensure the physics setup is a kinematic rigidbody trigger
            PickupRigidbody.isKinematic = true;
            m_Collider.isTrigger = true;

            // Remember start position for animation
            m_StartPosition = transform.position;
        }

        private void Update()
        {
            // Handle bobbing
            float bobbingAnimationPhase = (Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f + 0.5f) * BobbingAmount;
            transform.position = m_StartPosition + Vector3.up * bobbingAnimationPhase;

            // Handle rotating
            transform.Rotate(Vector3.up, RotatingSpeed * Time.deltaTime, Space.Self);
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerCharacterController pickingPlayer = other.GetComponent<PlayerCharacterController>();

            if (pickingPlayer != null)
            {
                OnPicked(pickingPlayer);

                PickupEvent evt = Events.PickupEvent;
                evt.Pickup = gameObject;
                EventManager.Broadcast(evt);

                Instigator playerInstigator = other.GetComponent<Instigator>();
                PlayerWeaponsManager playerWeaponsManager = other.GetComponent<PlayerWeaponsManager>();
                WeaponController playerActiveWeapon = playerWeaponsManager.GetActiveWeapon();
                Health playerHealth = other.GetComponent<Health>();

                ItemPickedUpTelemetryData evtData =
                    new(playerInstigator != null ? playerInstigator.Id : -1,
                        TelemetryConverterUtility.ConvertToTelemetryPickupType(m_type),
                        playerActiveWeapon != null ? playerActiveWeapon.GetCurrentAmmo() : -1,
                        playerHealth != null ? playerHealth.CurrentHealth : -1,
                        other.transform.position);
                TelemetryService.TrackItemPickUp(SessionManager.Instance.SessionID.ToString(), evtData);
            }
        }

        public void PlayPickupFeedback()
        {
            if (m_HasPlayedFeedback)
            {
                return;
            }

            if (PickupSfx)
            {
                AudioUtility.CreateSFX(PickupSfx, transform.position, AudioUtility.AudioGroups.Pickup, 0f);
            }

            if (PickupVfxPrefab)
            {
                GameObject pickupVfxInstance = Instantiate(PickupVfxPrefab, transform.position, Quaternion.identity);
            }

            m_HasPlayedFeedback = true;
        }

        protected virtual void OnPicked(PlayerCharacterController playerController)
        {
            PlayPickupFeedback();
        }
    }
}