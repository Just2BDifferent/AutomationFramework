namespace AutomationFramework
{
    public class BrowserHelper : IBrowserHelper
    {
        private IDictionary<Tuple<string, bool>, IBrowser> _browsers;
        private IPlaywright _playwright;
        public BrowserHelper(IPlaywright playwright)
        {
            _browsers = new Dictionary<Tuple<string, bool>, IBrowser>();
            _playwright = playwright;
        }

        public async Task<IBrowser> GetBrowser(string BrowserType, bool IsVisible = false)
        {
            Tuple<string, bool> browserKey = new Tuple<string, bool>(BrowserType, IsVisible);
            if (_browsers.ContainsKey(browserKey))
                return _browsers[browserKey];

            BrowserTypeLaunchOptions launchOptions = new BrowserTypeLaunchOptions();
            launchOptions.Headless = !IsVisible;

            if (BrowserType == "Edge")
            {
                launchOptions.Channel = "msedge";
                BrowserType = "Chromium";
            }
            
            IBrowser browser = await _playwright[BrowserType].LaunchAsync(launchOptions);
            _browsers.Add(browserKey, browser);
            return browser;
        }
    }
}
