namespace Unity.FPS.DDA
{
    public static class DDAConfig
    {
        public const string K_PENDING_RETRY_ENCOUNTER_ID = "DDA_PendingRetryEncounterId";

        public const string
            K_SERVER_URL = "http://localhost:8000/api/process_encounter/test"; //TODO: Remover antes de comit

        public const string K_PENDING_LEVEL_TRANSITION_ENCOUNTER_ID = "DDA_PendingLevelTransitionEncounterId";
        public const string K_PENDING_LEVEL_TRANSITION_RESTRICTIONS = "DDA_PendingLevelTransitionRestrictions";
    }
}