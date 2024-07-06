using System.Text.RegularExpressions;

namespace AutomationFramework
{
    public interface IPageRoute
    {
        public Regex TargetUrl { get; set; }
        public string PageURL { get; set; }
        public int Weight { get; set; }
        public void Route(IWebDevice device);
        public event EventHandler<RoutingEventArgs>? Routing;
    }
}
