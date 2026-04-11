using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telemetry.Events;
using Telemetry.Uploaders;

namespace Telemetry
{
    public class TelemetryBatcher : IDisposable, IAsyncDisposable
    {
        private readonly List<ITelemetryEvent> _batch = new();
        private readonly int _batchSize;
        private readonly ITelemetryUploader _uploader;
        private readonly object _lock = new();
        private bool _isFlushing;

        public TelemetryBatcher(int batchSize, ITelemetryUploader uploader)
        {
            _batchSize = batchSize;
            _uploader = uploader;
            _isFlushing = false;


            TelemetryEventBus.Subscribe(OnEventReceived);
        }

        public async Task FlushAsync()
        {
            List<ITelemetryEvent> events;
            lock (_lock)
            {
                if (_isFlushing) return;
                _isFlushing = true;

                int minElements = Math.Min(_batchSize, _batch.Count);
                events = _batch.GetRange(0, minElements);
                _batch.RemoveRange(0, minElements);
            }

            try
            {
                await _uploader.UploadAsync(events);
            }
            finally
            {
                lock (_lock)
                {
                    _isFlushing = false;
                }
            }
        }

        public void Dispose()
        {
            TelemetryEventBus.Unsubscribe(OnEventReceived);
            _batch.Clear();
        }

        public ValueTask DisposeAsync()
        {
            TelemetryEventBus.Unsubscribe(OnEventReceived);
            _batch.Clear();
            return new ValueTask();
        }

        private void OnEventReceived(ITelemetryEvent evt)
        {
            lock (_lock)
            {
                _batch.Add(evt);
                if (_batch.Count < _batchSize) return;
            }


            _ = FlushAsync();
        }
    }
}