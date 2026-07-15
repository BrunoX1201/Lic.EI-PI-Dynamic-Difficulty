using Unity.FPS.DDA;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class DifficultyHUD : MonoBehaviour
    {
        [Tooltip("Image component for the difficulty sprites")]
        public Image DifficultyImage;

        [Tooltip("Sprite to display when difficulty is MuchEasier")]
        public Sprite MuchEasierSprite;

        [Tooltip("Sprite to display when difficulty is Easier")]
        public Sprite EasierSprite;

        [Tooltip("Sprite to display when difficulty is the Same")]
        public Sprite SameSprite;

        [Tooltip("Sprite to display when difficulty is Harder")]
        public Sprite HarderSprite;

        [Tooltip("Sprite to display when difficulty is MuchHarder")]
        public Sprite MuchHarderSprite;

        private DDADirection Action;

        private void Start()
        {
            DDAEventManager.AddListener<DDAModelOutputReceived>(OnDifficultyChanged);
            DifficultyImage.sprite = SameSprite;

            if (DDAModifierState.HasReceivedOutput)
                UpdateSprite(DDAModifierState.LastAction);
        }

        private void OnDestroy()
        {
            DDAEventManager.RemoveListener<DDAModelOutputReceived>(OnDifficultyChanged);
        }

        private void OnDifficultyChanged(DDAModelOutputReceived evt)
        {
            UpdateSprite(evt.Action);
        }

        private void UpdateSprite(DDADirection action)
        {
            if (DifficultyImage == null) return;

            DifficultyImage.sprite = action switch
            {
                DDADirection.MuchEasier => MuchEasierSprite,
                DDADirection.Easier => EasierSprite,
                DDADirection.Same => SameSprite,
                DDADirection.Harder => HarderSprite,
                DDADirection.MuchHarder => MuchHarderSprite,
                _ => SameSprite
            };
        }
    }
}