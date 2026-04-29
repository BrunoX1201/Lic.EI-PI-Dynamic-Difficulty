using System;
using System.Collections.Generic;

namespace Unity.FPS.Telemetry
{
    public class FileTelemetryUploaderSettings
    {
        private const string k_fileNameSuffix = "event";
        public string BaseFilePath { get; private set; }

        private readonly Dictionary<Type, string> m_eventTypeToFileNameMap = new()
        {
            { typeof(GameTimePassedTelemetry), "game_time_passed" },
            { typeof(ItemPickedUpTelemetry), "item_picked_up" },
            { typeof(NewLocationDiscoveredTelemetry), "new_location_discovered" },
            { typeof(PlayerAttackedTelemetry), "player_attacked" },
            { typeof(PlayerDiedTelemetry), "player_died" },
            { typeof(PlayerTookDamageTelemetry), "player_took_damage" },
            { typeof(TargetKilledTelemetry), "target_killed" },
            { typeof(PlayerWentAirborneTelemetry), "player_went_airborne" },
            { typeof(EncounterStartedTelemetry), "encounter_started" },
            { typeof(EncounterEndedTelemetry), "encounter_ended" },
            { typeof(EncounterZoneEnteredTelemetry), "encounter_zone_entered" },
            { typeof(EncounterZoneLeftTelemetry), "encounter_zone_left" }
        };

        public FileTelemetryUploaderSettings(string baseFilePath)
        {
            BaseFilePath = baseFilePath;
        }

        public string GetFileNameForEvent(ITelemetryEvent evt)
        {
            string basePath = m_eventTypeToFileNameMap.GetValueOrDefault(evt.GetType(), "unknown");
            return $"{basePath}_{k_fileNameSuffix}";
        }

        public string GetFileNameForEvent(Type evtType)
        {
            string basePath = m_eventTypeToFileNameMap.GetValueOrDefault(evtType, "unknown");
            return $"{basePath}_{k_fileNameSuffix}";
        }

        public List<Type> GetAllEvents()
        {
            List<Type> types = new();
            if (m_eventTypeToFileNameMap != null)
            {
                types.AddRange(m_eventTypeToFileNameMap.Keys);
            }

            return types;
        }
    }
}