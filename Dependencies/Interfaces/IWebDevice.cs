using Deque.AxeCore.Commons;

namespace AutomationFramework
{
    public interface IWebDevice
    {
        public IScenarioContext Scenario { get; }
        public bool IsVisible { get; set; }
        public void SetDeviceType(string deviceType, bool? IsVisible = null);
        public Task GoDirectlyToAsync(string url, PageGotoOptions? gotoOptions = null);
        public Task NavigateToAsync(string url);
        public IPageAssertions CurrentPage();
        public ILocatorAssertions CurrentLocator();
        public ILocator Locate(string selector, LocatorLocatorOptions? locatorLocatorOptions = null, bool LimitToCurrentSelector = false);
        public ILocator Locate(AriaRole role, LocatorGetByRoleOptions? options = null, bool LimitToCurrentSelector = false);
        public ILocator Locate(ILocator locator, bool LimitToCurrentSelector = false);

        public ILocator GetLocator(string selector, PageLocatorOptions? pageLocatorOptions = null);
        public ILocator GetLocator(AriaRole ariaRole, PageGetByRoleOptions? pageLocatorOptions = null);


        public Task PauseAsync();
        public Task<AxeResult> CheckAODA(bool onLocatorOnly = false);
        public event EventHandler<IDownload> OnDownload;

        public IPageModel? GetPageModel();
    }
}
