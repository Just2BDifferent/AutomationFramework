namespace AutomationFramework
{

    public static class DependencyInjection
    {
        public static Task<IPlaywright> _playwrightTask = Playwright.CreateAsync();

        [ScenarioDependencies]
        public static IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IPlaywright>(_playwrightTask.Result);
            services.AddSingleton<IBrowserHelper, BrowserHelper>();
            services.AddScoped<IWebDevice, WebDevice>();
            return services;
        }
    }
}
