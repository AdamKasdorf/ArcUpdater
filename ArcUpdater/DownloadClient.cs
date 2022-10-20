using System;
using System.Net.Http;

namespace ArcUpdater
{
#if NET6_0_OR_GREATER
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class DownloadClient
    {
        private static readonly IHttpClientFactory HttpClientFactory;

        static DownloadClient()
        {
            IHostBuilder builder = new HostBuilder()
                .ConfigureServices(ConfigureHttpClientServices)
                .UseConsoleLifetime();

            IHost host = builder.Build();
            HttpClientFactory = host.Services.GetService<IHttpClientFactory>();
        }

        private static void ConfigureHttpClientServices(IServiceCollection services)
        {
            services.AddHttpClient();
        }

        public DownloadClient()
        {
        }

        public HttpClient Create()
        {
            if (HttpClientFactory == null)
            {
                throw new InvalidOperationException();
            }

            return HttpClientFactory.CreateClient();
        }
    }
}
#else
    public class DownloadClient : IDisposable
    {
        private HttpClientHandler _httpClientHandler;
        private bool _disposed;

        public DownloadClient()
        {
            _httpClientHandler = new HttpClientHandler();
        }

        ~DownloadClient()
        {
            _httpClientHandler.Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_httpClientHandler != null)
            {
                _httpClientHandler.Dispose();
                _httpClientHandler = null;
            }

            GC.SuppressFinalize(this);
        }

        public HttpClient Create()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DownloadClient));
            }

            return new HttpClient(_httpClientHandler, false);
        }
    }
}
#endif