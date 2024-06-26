using System.Text.RegularExpressions;

namespace AutomationFramework
{
    internal class PageHelper : IPageHelper
    {
        public IDictionary<Regex, IPageModel> Pages { get; set; }

        public PageHelper()
        {
            Pages = new Dictionary<Regex, IPageModel>();

            var interfaceType = typeof(IPageModel);
            var pomTypes = AppDomain.CurrentDomain.GetAssemblies()
              .SelectMany(x => x.GetTypes())
              .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
              .Select(x => Activator.CreateInstance(x) as IPageModel);

            foreach (var pom in pomTypes)
            {
                if (pom != null)
                {
                    Pages.Add(pom.PageUrlRegex, pom);
                }
            }
        }
        
    }
}
