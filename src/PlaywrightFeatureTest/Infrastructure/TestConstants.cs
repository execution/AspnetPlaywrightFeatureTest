using Microsoft.Extensions.Logging;
using PlaywrightSharp;
using System;

namespace PlaywrightFeatureTest.Infrastructure
{
    internal static class TestConstants
    {
        public const string ChromiumProduct = "CHROMIUM";
        public const string WebkitProduct = "WEBKIT";
        public const string FirefoxProduct = "FIREFOX";
        public const int DefaultTestTimeout = 30_000;
        public const int DefaultPuppeteerTimeout = 10_000;
        public const int DefaultTaskTimeout = 5_000;
        public const string TestFixtureBrowserCollectionName = "ShareWebServer";

        public static string Product => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PRODUCT")) ?
          ChromiumProduct :
          Environment.GetEnvironmentVariable("PRODUCT");

        internal static ILoggerFactory LoggerFactory { get; set; } = LoggerFactory = new LoggerFactory();
        internal static LaunchOptions GetDefaultBrowserOptions()
           => new LaunchOptions
           {
               SlowMo = Convert.ToInt32(Environment.GetEnvironmentVariable("SLOW_MO")),
               Headless = Convert.ToBoolean(Environment.GetEnvironmentVariable("HEADLESS") ?? "false"),
               Timeout = 0,
           };
    }
}
