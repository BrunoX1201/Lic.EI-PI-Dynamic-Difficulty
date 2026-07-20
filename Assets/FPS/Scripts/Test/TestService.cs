using System.Diagnostics;
using System.Threading.Tasks;
using Unity.FPS.API;
using Unity.FPS.DDA;
using Debug = UnityEngine.Debug;

namespace Unity.FPS.Test
{
    public class TestService
    {
        private const string m_testsDataPath = "data/shared/tests";
        public bool Initialized { get; private set; }

        public async Task Initialize(string testingSessionID, bool isRestart = false)
        {
            if (Initialized)
            {
                Debug.Log("[TEST] Service already initialized");
                return;
            }

            Debug.Log("[TEST] Service initializing...");
            if (isRestart)
            {
                await APIService.Instance.Restart(testingSessionID, m_testsDataPath);
            }
            else
            {
                await APIService.Instance.Initialize(testingSessionID, m_testsDataPath);
            }

            if (!APIService.Instance.Initialized)
            {
                Debug.LogError("[TEST] Service could not be initialized");
                return;
            }

            Debug.Log("[TEST] Service initialized");
            Initialized = true;
        }

        public async Task Restart(string testingSessionID)
        {
            Initialized = false;
            Debug.Log("[TEST] Service restarting...");
            await Initialize(testingSessionID, true);
        }

        public async Task<TestResult<float>> TestDDAProcessEncounter()
        {
            string resultName = "Average Response Time MS";
            float resultValue = -1;
            if (!Initialized)
            {
                return new TestResult<float>(resultName, resultValue, false);
            }

            long totalTimeMS = 0;
            int iterations = 100;
            DDAEncounterRestrictionsSO encounterRestrictions = new();
            ProcessEncounterRequest request = new()
            {
                EncounterId = "01-001",
                NextEncounterRestrictions = new EncounterRestrictions
                {
                    Turret = new EnemyTypeRestrictions
                    {
                        Count = new LimitValue<int>(encounterRestrictions.MinBosses, encounterRestrictions.MaxBosses,
                            encounterRestrictions.DefaultBosses,
                            encounterRestrictions.DefaultBosses),
                        Health = new LimitValue<float>(encounterRestrictions.MinTurretHealth,
                            encounterRestrictions.MaxTurretHealth,
                            encounterRestrictions.DefaultTurretHealth, encounterRestrictions.DefaultTurretHealth),
                        Hitbox = new LimitValue<float>(encounterRestrictions.MinTurretHitbox,
                            encounterRestrictions.MaxTurretHitbox,
                            encounterRestrictions.DefaultTurretHitbox, encounterRestrictions.DefaultTurretHitbox)
                    },
                    Mobile = new EnemyTypeRestrictions
                    {
                        Count = new LimitValue<int>(encounterRestrictions.MinMobiles, encounterRestrictions.MaxMobiles,
                            encounterRestrictions.DefaultMobiles,
                            encounterRestrictions.DefaultMobiles),
                        Health = new LimitValue<float>(encounterRestrictions.MinMobileHealth,
                            encounterRestrictions.MaxMobileHealth,
                            encounterRestrictions.DefaultMobileHealth, encounterRestrictions.DefaultMobileHealth),
                        Hitbox = new LimitValue<float>(encounterRestrictions.MinMobileHitbox,
                            encounterRestrictions.MaxMobileHitbox,
                            encounterRestrictions.DefaultMobileHitbox, encounterRestrictions.DefaultMobileHitbox)
                    }
                },
                Options = new ProcessEncounterOptions
                {
                    RollbackOnSuccess = true
                }
            };

            Stopwatch stopwatch = new();
            resultValue = 0;
            for (int i = 0; i < iterations; i++)
            {
                stopwatch.Start();
                APIResult<ProcessEncounterResponse> result = await APIService.ProcessEncounter(request);
                stopwatch.Stop();

                if (!result.Success)
                {
                    return new TestResult<float>(resultName, -1, false);
                }

                long nowMS = stopwatch.ElapsedMilliseconds;
                totalTimeMS += nowMS;

                stopwatch.Reset();
            }

            resultValue = totalTimeMS / iterations;
            return new TestResult<float>(resultName, resultValue, true);
        }
    }
}