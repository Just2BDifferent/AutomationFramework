using System.Text.RegularExpressions;

namespace AutomationFramework
{
    public interface IPageHelper
    {
        public IDictionary<Regex, IPageModel> Pages { get; }
    }
}
