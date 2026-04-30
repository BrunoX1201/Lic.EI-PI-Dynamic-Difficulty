using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.FPS.Telemetry
{
    public class TelemetryBatcher : IDisposable, IAsyncDisposable
    {
        private readonly List<ITelemetryEvent> m_batch = new();
        private readonly int m_batchSize;
        private readonly ITelemetryUploader m_uploader;
        private readonly object m_lock = new();
        private bool m_isFlushing;

        public TelemetryBatcher(int batchSize, ITelemetryUploader uploader)
        {
            m_batchSize = batchSize;
            m_uploader = uploader;
            m_isFlushing = false;


            TelemetryEventBus.Subscribe(OnEventReceived);
        }

        public async Task FlushAsync()
        {
            List<ITelemetryEvent> events;
            lock (m_lock)
            {
                if (m_isFlushing)
                {
                    return;
                }

                m_isFlushing = true;

                int minElements = Math.Min(m_batchSize, m_batch.Count);
                events = m_batch.GetRange(0, minElements);
                m_batch.RemoveRange(0, minElements);
            }

            try
            {
                await m_uploader.UploadAsync(events);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                lock (m_lock)
                {
                    m_isFlushing = false;

                    if (m_batch.Count >= m_batchSize)
                    {
                        _ = FlushAsync();
                    }
                }
            }
        }

        public void Dispose()
        {
            TelemetryEventBus.Unsubscribe(OnEventReceived);
            m_batch.Clear();
            m_uploader.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            TelemetryEventBus.Unsubscribe(OnEventReceived);
            m_batch.Clear();
            await m_uploader.DisposeAsync();
        }

        private void OnEventReceived(ITelemetryEvent evt)
        {
            lock (m_lock)
            {
                m_batch.Add(evt);
                if (m_batch.Count < m_batchSize)
                {
                    return;
                }
            }


            _ = FlushAsync();
        }
    }
}