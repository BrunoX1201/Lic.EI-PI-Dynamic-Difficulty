using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.FPS.Telemetry
{
    public class CSVFileTelemetryUploader : ITelemetryUploader
    {
        private readonly FileTelemetryUploaderSettings m_settings;
        private readonly Dictionary<Type, StreamWriter> m_writers = new();

        public CSVFileTelemetryUploader(FileTelemetryUploaderSettings settings)
        {
            m_settings = settings;

            foreach (Type evt in m_settings.GetAllEvents())
            {
                if (m_writers.ContainsKey(evt))
                {
                    continue;
                }

                FileStream fileStream = new(GetEventFileName(evt), FileMode.Append, FileAccess.Write, FileShare.Read);
                StreamWriter sw = new(fileStream);
                m_writers.Add(evt, sw);
            }
        }

        public Task UploadAsync(IEnumerable<ITelemetryEvent> events)
        {
            foreach (ITelemetryEvent evt in events)
            {
                try
                {
                    if (!m_writers.ContainsKey(evt.GetType()))
                    {
                        throw new Exception($"Event {evt.GetType().Name} does not exist in stream writers dictionary!");
                    }

                    StreamWriter sw = m_writers[evt.GetType()];
                    StringBuilder lineBuilder = new();

                    int i = 0;
                    for (; i < evt.Data.Count - 1; i++)
                    {
                        lineBuilder.Append(m_settings.ConvertEventDataToString(evt.Data.ElementAt(i).Value) + ";");
                    }

                    lineBuilder.Append(m_settings.ConvertEventDataToString(evt.Data.ElementAt(i).Value));

                    sw.WriteLine(lineBuilder.ToString());
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            foreach (KeyValuePair<Type, StreamWriter> element in m_writers)
            {
                element.Value.Flush();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<Type, StreamWriter> element in m_writers)
            {
                element.Value.Close();
            }
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask();
        }

        private string GetEventFileName(ITelemetryEvent evt)
        {
            return $@"{m_settings.BaseFilePath}\{m_settings.GetFileNameForEvent(evt)}.csv";
        }

        private string GetEventFileName(Type evtType)
        {
            return $@"{m_settings.BaseFilePath}\{m_settings.GetFileNameForEvent(evtType)}.csv";
        }
    }
}