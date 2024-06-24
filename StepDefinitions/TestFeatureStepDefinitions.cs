using System;
using Reqnroll;

namespace AutomationFramework.StepDefinitions
{
    [Binding]
    public class TestFeatureStepDefinitions
    {
        public IWebDevice Device { get; set; }
        public TestFeatureStepDefinitions(IWebDevice webDevice) { Device = webDevice; }

        [Given("I go to (.*)")]
        public async Task GivenIGoTo(string url)
        {
            await Device.GotoAsync(url);
        }

        [When("I search for (.*)")]
        public async Task WhenISearchFor(string searchString)
        {
            var searchBar = Device.LocateByRole(AriaRole.Combobox, new() { NameString = "Search"});
            await searchBar.FillAsync(searchString);
            await searchBar.PressAsync("Enter");
        }

        [Then("I can find a (link|heading|textbox|button) with text (.*)")]
        public async Task ThenICanFindAAriaTypeWithTextLabelText(AriaRole role, string LabelText)
        {
            Device.LocateByRole(role, new() { NameString = LabelText });
            await Device.CurrentLocator().ToBeVisibleAsync();
            await Device.PauseAsync();
        }
    }
}
