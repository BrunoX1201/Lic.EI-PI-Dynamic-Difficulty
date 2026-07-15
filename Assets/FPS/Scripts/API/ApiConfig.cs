namespace Unity.FPS.API
{
    public static class APIConfig
    {
        public static readonly string API_BASE_URL = "http://127.0.0.1:8000";
        public static readonly string SESSION_ENDPOINT = $"{API_BASE_URL}/session";
        public static readonly string PROCESS_ENCOUNTER_ENDPOINT = $"{API_BASE_URL}/process_encounter";
        public static readonly string DDA_PIPELINE_ENDPOINT = $"{API_BASE_URL}/dda_pipeline";
    }
}