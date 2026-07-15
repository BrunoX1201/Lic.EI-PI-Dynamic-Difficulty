using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class ObjectiveReachPoint : Objective
    {
        [Tooltip("Visible transform that will be destroyed once the objective is completed")]
        public Transform DestroyRoot;

        private void Awake()
        {
            if (DestroyRoot == null)
                DestroyRoot = transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsCompleted)
                return;

            PlayerCharacterController player = other.GetComponent<PlayerCharacterController>();
            // test if the other collider contains a PlayerCharacterController, then complete
            if (player != null)
            {
                CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);

                // destroy the transform, will remove the compass marker if it has one
                Destroy(DestroyRoot.gameObject);
            }
        }

        public override void ForceCompletion()
        {
            CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);
        }
    }
}