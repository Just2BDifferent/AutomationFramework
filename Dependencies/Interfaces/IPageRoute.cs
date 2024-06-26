using System.Text.RegularExpressions;

namespace AutomationFramework
{
    public interface IPageRoute
    {
        public Regex TargetUrl { get; set; }
        public int Weight { get; set; }
        public void Route();
        public event EventHandler Routing;
    }
}
