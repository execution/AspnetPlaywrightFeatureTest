using PlaywrightFeatureTest.Infrastructure;
using System.Threading.Tasks;
using Xunit;


namespace PlaywrightFeatureTest
{
    [Collection(TestConstants.TestFixtureBrowserCollectionName)]
    public class HomeControllerTests: PlaywrightSharpBaseTest
    {
        public HomeControllerTests(WebHostServerFixture server) : base(server)
        {
        }
        [Fact]
        public async Task ShouldBeAbleLandToIndex()
        {
            var url = Server.RootUri.AbsoluteUri;
            var fullUrl = $"{url}home/index";
            var res = await Page.GoToAsync(fullUrl);
            Assert.Contains("Welcome", await res.GetTextAsync());
        } 
        [Fact]
        public async Task ShouldBeAbleLandToPrivacy()
        {
            var url = Server.RootUri.AbsoluteUri;
            var fullUrl = $"{url}home/privacy";
            var res = await Page.GoToAsync(fullUrl);
            Assert.Contains("Use this page to detail your site's privacy policy.", await res.GetTextAsync());
        }
    }
}
