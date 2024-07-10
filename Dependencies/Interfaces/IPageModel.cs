using System.Text.RegularExpressions;

namespace AutomationFramework
{
    public interface IPageModel
    {
        public Regex PageUrlRegex { get; set; }
        public string PageUrl { get; set; }
        public ILocator GetLocator(IWebDevice webDevice, string locatorName);
        public IList<IPageRoute> Routes { get; set; }

        public bool IsStartPage { get; }
    }
}
