using Deque.AxeCore.Commons;

namespace AutomationFramework
{
    public interface IWebDevice
    {
        public bool IsVisible { get; set; }
        public void SetDeviceType(string deviceType, bool? IsVisible = null);
        public Task GoDirectlyToAsync(string url, PageGotoOptions? gotoOptions = null);
        public Task NavigateToAsync(string url);
        public IPageAssertions CurrentPage();
        public ILocatorAssertions CurrentLocator();
        public ILocator Locate(string selector, LocatorLocatorOptions? locatorLocatorOptions = null);
        public ILocator Locate(AriaRole role, LocatorGetByRoleOptions? options = null);
        public Task PauseAsync();
        public Task<AxeResult> CheckAODA(bool onLocatorOnly = false);
        public event EventHandler<IDownload> OnDownload;
    }
}
