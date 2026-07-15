using Unity.FPS.DDA;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class DDAManager : MonoBehaviour
    {
        private void Awake()
        {
            DDAService.Instance.Initialize();
        }
    }
}