namespace AutomationFramework
{
    public interface IBrowserHelper
    { 
        public Task<IBrowser> GetBrowser(string BrowsersName, bool IsVisible = false);
    }
}
