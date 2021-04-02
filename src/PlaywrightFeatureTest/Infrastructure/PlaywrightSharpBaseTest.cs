using PlaywrightSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PlaywrightFeatureTest.Infrastructure
{
    public abstract class PlaywrightSharpBaseTest : IDisposable, IAsyncLifetime
    {
        internal IPlaywright Playwright => WebHostServerFixture.Playwright;
        internal IBrowserType BrowserType => Playwright[TestConstants.Product];
        internal WebHostServerFixture Server;
        internal IBrowser Browser => WebHostServerFixture.Browser;
        internal LaunchOptions DefaultOptions { get; set; }
        internal IBrowserContext Context { get; set; }
        protected IPage Page { get; set; }

        public PlaywrightSharpBaseTest(WebHostServerFixture server) => Server = server;

        protected async Task<IPage> NewPageAsync(IBrowser browser, BrowserContextOptions options = null)
        {
            var context = await browser.NewContextAsync(options);
            return await context.NewPageAsync();
        }

        public virtual async Task DisposeAsync()
        {
            await Context.CloseAsync();
        }

        public virtual async Task InitializeAsync()
        {
            Context = await Browser.NewContextAsync();
            Context.DefaultTimeout = TestConstants.DefaultPuppeteerTimeout;
            Page = await Context.NewPageAsync();
        }
        protected Task WaitForError()
        {
            var wrapper = new TaskCompletionSource<bool>();

            void errorEvent(object sender, EventArgs e)
            {
                wrapper.SetResult(true);
                Page.Crash -= errorEvent;
            }

            Page.Crash += errorEvent;

            return wrapper.Task;
        }

        public virtual void Dispose()
        {
        }
    }
}
