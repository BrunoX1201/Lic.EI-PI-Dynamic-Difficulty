using Unity.FPS.API;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class APIManager : MonoBehaviour
    {
        private void Start()
        {
            APIService.Instance.Initialize(SessionManager.Instance.SessionID.ToString());
        }
    }
}