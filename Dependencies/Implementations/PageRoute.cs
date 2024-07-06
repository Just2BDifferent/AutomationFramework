using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutomationFramework
{
    public class PageRoute : IPageRoute
    {
        public Regex TargetUrl { get; set; }
        public string PageURL { get; set; }
        public int Weight { get; set; }

        public event EventHandler<RoutingEventArgs>? Routing;

        public void Route(IWebDevice device)
        {
            Routing?.Invoke(device, new RoutingEventArgs() {});
        }

        public PageRoute(Regex targetUrl, string pageURL, int weight)
        {
            TargetUrl = targetUrl;
            PageURL = pageURL;
            Weight = weight;
        }
    }

    public class RoutingEventArgs : EventArgs
    {

    }
}


