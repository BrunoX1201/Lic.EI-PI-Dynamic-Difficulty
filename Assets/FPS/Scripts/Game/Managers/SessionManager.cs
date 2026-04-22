using System;

namespace Unity.FPS.Game
{
    public class SessionManager : Singleton<SessionManager>
    {
        public Guid SessionID { get; private set; }

        public override void Awake()
        {
            base.Awake();

            GenerateSessionID();
        }

        private void GenerateSessionID()
        {
            SessionID = Guid.NewGuid();
        }
    }
}