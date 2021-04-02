using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlaywrightSharp;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PlaywrightFeatureTest.Infrastructure
{
    [CollectionDefinition(TestConstants.TestFixtureBrowserCollectionName, DisableParallelization = true)]
    public class ShareWebserver : ICollectionFixture<WebHostServerFixture>
    {
    }
    public class WebHostServerFixture : IDisposable, IAsyncLifetime
    {
        internal static IPlaywright Playwright { get; private set; }
        internal static IBrowser Browser { get; private set; }
        private readonly Lazy<Uri> _rootUriInitializer;
        public Uri RootUri => _rootUriInitializer.Value;
        public IHost Host { get; set; }

        public WebHostServerFixture()
        {
            _rootUriInitializer = new Lazy<Uri>(() => new Uri(StartAndGetRootUri()));
        }

        protected static void RunInBackgroundThread(Action action)
        {
            using var isDone = new ManualResetEvent(false);

            ExceptionDispatchInfo edi = null;
            new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    edi = ExceptionDispatchInfo.Capture(ex);
                }

                isDone.Set();
            }).Start();

            if (!isDone.WaitOne(TimeSpan.FromSeconds(150)))
                throw new TimeoutException("Timed out waiting for: " + action);

            if (edi != null)
                throw edi.SourceException;
        }

        protected string StartAndGetRootUri()
        {
            // As the port is generated automatically, we can use IServerAddressesFeature to get the actual server URL
            Host = CreateWebHost();
            RunInBackgroundThread(Host.Start);
            return Host.Services.GetRequiredService<IServer>().Features
                .Get<IServerAddressesFeature>()
                .Addresses.FirstOrDefault();
        }
        /// <inheritdoc/>
        public Task InitializeAsync() => LaunchBrowserAsync();

        /// <inheritdoc/>
        public Task DisposeAsync() => ShutDownAsync();

        private async Task LaunchBrowserAsync()
        {
            try
            {
                Playwright = await PlaywrightSharp.Playwright.CreateAsync(TestConstants.LoggerFactory, debug: "pw*");
                Browser = await Playwright[TestConstants.Product].LaunchAsync(TestConstants.GetDefaultBrowserOptions());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new Exception("Launch failed", ex);
            }
        }

        internal async Task ShutDownAsync()
        {
            try
            {
                await Browser.CloseAsync();
                Playwright.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new Exception("Shutdown failed", ex);
            }
        }

        public void Dispose()
        {
            Host?.Dispose();
            Host?.StopAsync();
        }

        protected IHost CreateWebHost()
        {
            var osPath = Path.DirectorySeparatorChar;
            var path = $"..{osPath}..{osPath}";            
           
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .CreateLogger();
            var host = new HostBuilder()
                .ConfigureWebHost(webHostBuilder => webHostBuilder
                    .UseKestrel()
                    .UseSolutionRelativeContentRoot(path, "AspnetPlaywrightFeatureTest.sln")                    
                    .UseStaticWebAssets()
                    .UseStartup<WebUnderTest.Startup>()
                    .UseSerilog() 
                    .UseUrls($"http://127.0.0.1:0"))
                .Build();

            return host;
        }
    }
}
