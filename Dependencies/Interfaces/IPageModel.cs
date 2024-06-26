using System.Text.RegularExpressions;

namespace AutomationFramework
{
    public interface IPageModel
    {
        public Regex PageUrlRegex { get; set; }
        public string PageUrl { get; set; }
        public IDictionary<string, Tuple<AriaRole, ILocator>> Locators { get; set; }
        public IList<IPageRoute> Routes { get; set; }
    }
}
