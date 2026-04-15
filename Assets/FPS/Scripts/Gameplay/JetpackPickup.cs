namespace Unity.FPS.Gameplay
{
    public class JetpackPickup : Pickup
    {
        protected override void Start()
        {
            base.Start();
            m_type = PickupType.Jetpack;
        }

        protected override void OnPicked(PlayerCharacterController byPlayer)
        {
            Jetpack jetpack = byPlayer.GetComponent<Jetpack>();
            if (!jetpack)
            {
                return;
            }

            if (jetpack.TryUnlock())
            {
                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }
    }
}