using System.Threading.Tasks;

namespace Unity.FPS.API
{
    public interface IApiClient
    {
        Task<ApiResult<IProcessEncounterResponse>> ProcessEncounter(IProcessEncounterRequest request);
    }
}