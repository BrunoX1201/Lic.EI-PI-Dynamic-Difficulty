using System;

namespace Unity.FPS.Game
{
    public class SessionManager : Singleton<SessionManager>
    {
        public Guid SessionID { get; private set; }

        public SessionManager()
        {
            GenerateSessionID();
        }

        private void GenerateSessionID()
        {
            SessionID = Guid.NewGuid();
        }
    }
}