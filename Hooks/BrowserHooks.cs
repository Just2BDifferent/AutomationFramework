namespace AutomationFramework.Hooks
{
    [Binding]
    public class BrowserHooks
    {

        [BeforeScenario(["iPhone"])]
        public static void UseiPhone(IWebDevice device)
        {
            device.SetDeviceType("iPhone 14");
        }

        [BeforeScenario(["iPhoneProMax"])]
        public static void UseiPhoneProMax(IWebDevice device)
        {
            device.SetDeviceType("iPhone 14 Pro Max");
        }

        [BeforeScenario("Edge")]
        public static void UseEdge(IWebDevice device)
        {
            device.SetDeviceType("Desktop Edge");
        }

        [BeforeScenario("Firefox")]
        public static void UseFirefox(IWebDevice device)
        {
            device.SetDeviceType("Desktop Firefox");
        }

        [BeforeScenario("Visible")]
        public static void SetVisibility(IWebDevice device)
        {
            device.IsVisible = true;
        }

    }
}
