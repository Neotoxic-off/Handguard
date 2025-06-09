using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Handguard.Lib
{
    public class ContentStreamer : HttpContent
    {
        private readonly HttpContent _originalContent;
        private readonly Action<long, double>? _progressWithSpeedCallback;
        private int _bufferSize = Settings.MinBufferSize;

        public ContentStreamer(HttpContent content, Action<long, double>? progressWithSpeedCallback = null)
        {
            _originalContent = content;
            _progressWithSpeedCallback = progressWithSpeedCallback;

            foreach (var header in _originalContent.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            long total = 0;
            byte[] buffer = new byte[_bufferSize];
            using Stream inputStream = await _originalContent.ReadAsStreamAsync();

            DateTime lastTime = DateTime.UtcNow;
            long lastBytes = 0;
            double lastSpeed = 0;

            int read;
            while ((read = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await stream.WriteAsync(buffer, 0, read);
                total += read;

                DateTime now = DateTime.UtcNow;
                double timeElapsed = (now - lastTime).TotalSeconds;

                if (timeElapsed >= 1.0)
                {
                    long bytesSinceLast = total - lastBytes;
                    lastSpeed = bytesSinceLast / timeElapsed;

                    lastBytes = total;
                    lastTime = now;
                }

                _progressWithSpeedCallback?.Invoke(total, lastSpeed);

                if (_bufferSize < Settings.MaxBufferSize)
                {
                    _bufferSize = Math.Min(_bufferSize * 2, Settings.MaxBufferSize);
                }
            }
        }


        protected override bool TryComputeLength(out long length)
        {
            if (_originalContent.Headers.ContentLength.HasValue)
            {
                length = _originalContent.Headers.ContentLength.Value;
                return true;
            }
            length = -1;
            return false;
        }
    }
}
