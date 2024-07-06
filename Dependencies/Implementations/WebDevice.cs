﻿using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using Reqnroll.Assist.ValueComparers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Timers;

namespace AutomationFramework
{
    public class WebDevice : IWebDevice, IDisposable
    {
        private readonly IPlaywright _playwright;
        private readonly IBrowserHelper _browserHelper;
        private readonly IPageHelper _pageHelper;

        private string _deviceType = "Chrome Desktop";
        private string _browserType = "Chromium";
        private BrowserTypeLaunchOptions _launchOptions = new BrowserTypeLaunchOptions();
        private BrowserNewContextOptions _contextOptions = new BrowserNewContextOptions();

        private Task<IBrowser>? _browser;
        private IBrowser Browser
        {
            get
            {
                if (_browser == null)
                {
                    _browser = LaunchBrowser();
                }
                return _browser.Result;
            }
        }

        private Task<IBrowserContext>? _browserContext;
        private IBrowserContext BrowserContext
        {
            get
            {
                if (_browserContext == null)
                {
                    _browserContext = CreateBrowserContext();
                }
                return _browserContext.Result;
            }
        }

        private Task<IPage>? _page;
        private IPage Page
        {
            get
            {
                if (_page == null)
                {
                    _page = OpenPage();
                }
                return _page!.Result;
            }
        }

        private ILocator? _locator;
        private ILocator Locator
        {
            get
            {
                if (_locator == null)
                {
                    _locator = Page.Locator("body");
                }
                return _locator;
            }
            set { _locator = value; }
        }

        public bool IsVisible { get; set; }
        public WebDevice(IPlaywright playwright, IBrowserHelper browserHelper, IPageHelper pageHelper)
        {
            _playwright = playwright;
            _browserHelper = browserHelper;
            _pageHelper = pageHelper;
        }

        public void SetDeviceType(string deviceType, bool? IsVisible = null)
        {
            if (IsVisible != null)
                this.IsVisible = IsVisible.Value;

            if (_playwright.Devices.ContainsKey(deviceType))
            {
                _deviceType = deviceType;
            }
            else
            {
                throw new ArgumentException($"Device type \"{deviceType}\" is not a valid Playwright Emulation Device ");
            }

            if (deviceType.Contains("iPhone") || deviceType.Contains("iPad") || deviceType.Contains("Safari"))
            {
                _browserType = "Safari";
            }
            else if (deviceType.Contains("Edge"))
            {
                _browserType = "Edge";
            }
            else if (deviceType.Contains("Firefox"))
            {
                _browserType = "Firefox";
            }
            else
            {
                _browserType = "Chromium";
            }

            if (_browserContext != null)
            {
                _browserContext = null;
            }
            if (_browser != null)
            {
                _browser = null;
            }
        }

        private Task<IBrowser> LaunchBrowser()
        {
            if (_browserContext != null)
            {
                _browserContext.Result.CloseAsync();
                _browserContext = null;
            }
            return _browserHelper.GetBrowser(_browserType, IsVisible);
        }

        private Task<IBrowserContext> CreateBrowserContext()
        {
            return Browser.NewContextAsync(_contextOptions);
        }

        private Task<IPage> OpenPage()
        {
            return BrowserContext.NewPageAsync();
        }

        public async Task GoDirectlyToAsync(string url, PageGotoOptions? gotoOptions = null) => await Page.GotoAsync(url, gotoOptions);

        public async Task NavigateToAsync(string url)
        {
            if (url == Page.Url)
                return;

            IPageModel? cur_page = _pageHelper.Pages.Where(x => x.Key.IsMatch(Page.Url)).FirstOrDefault().Value;

            if (cur_page == null)
            {
                await GoDirectlyToAsync(url);
                cur_page = _pageHelper.Pages.Where(x => x.Key.IsMatch(Page.Url)).FirstOrDefault().Value;
                if (cur_page == null || cur_page.PageUrlRegex.IsMatch(url))
                    return;
            }

            var next_route = cur_page.Routes.Where(x => x.TargetUrl.IsMatch(url)).FirstOrDefault();

            if (next_route != null)
            {
                next_route.Route(this);
                return;
            }
            CancellationTokenSource? cts = new CancellationTokenSource();
            ParallelOptions options = new()
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            int best_score = 100;
            object lockable = new object();
            IList<IPageRoute> routes = new List<IPageRoute>();

            try
            {
                Parallel.ForEach(cur_page.Routes, options, (route) =>
                {
                    List<IPageRoute> check_route = new List<IPageRoute>();
                    check_route.Add(route);

                    if (options.CancellationToken.IsCancellationRequested)
                        return;

                    var next_page = _pageHelper.Pages.Where(x => x.Key.IsMatch(route.PageURL)).FirstOrDefault().Value;

                    IPageRoute? nextRoute = next_page.Routes.Where(x => x.TargetUrl.IsMatch(url)).OrderBy(x => x.Weight).FirstOrDefault();

                    if (nextRoute != null)
                    {
                        check_route.Add(nextRoute);
                        if (check_route.Sum(x => x.Weight) < best_score)
                        {
                            lock (lockable)
                            {
                                best_score = check_route.Sum(x => x.Weight);
                                routes.Clear();
                                routes = check_route;
                                cts.CancelAfter(TimeSpan.FromSeconds(2));
                            }
                            return;
                        }
                    }

                    var winning_route = RecurseRoute(cts, ref lockable, ref best_score, check_route, url);

                    if (winning_route != null && winning_route.Sum(x => x.Weight) <= best_score)
                    {
                        lock (lockable)
                        {
                            best_score = winning_route.Sum(x => x.Weight);
                            routes.Clear();
                            routes = check_route;
                        }
                        return;
                    }

                });

                if (routes.Count > 0)
                {
                    for (int i = 0; i < routes.Count; i++)
                    {
                        routes[i].Route(this);
                    }
                }
                else
                {
                    Assert.Fail("Could not navigate to expected page");
                }
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                cts.Dispose();
            }

        }

        private List<IPageRoute>? RecurseRoute(CancellationTokenSource cts, ref object lockable, ref int best_score, List<IPageRoute> route, string url)
        {
            if (cts.Token.IsCancellationRequested)
                return null;

            var next_page = _pageHelper.Pages.Where(x => x.Key.IsMatch(route.Last().PageURL)).FirstOrDefault().Value;

            IPageRoute? nextRoute = next_page.Routes.Where(x => x.TargetUrl.IsMatch(url)).OrderBy(x => x.Weight).FirstOrDefault();

            if (nextRoute != null)
            {
                route.Add(nextRoute);
                if (route.Sum(x => x.Weight) < best_score)
                {
                    lock (lockable)
                    {
                        best_score = route.Sum(x => x.Weight);
                        cts.CancelAfter(TimeSpan.FromSeconds(2));
                    }
                    return route;
                }
                else
                {
                    return null;
                }
            }

            List<IPageRoute>? winning_route = null;

            foreach (var rt in next_page.Routes)
            {
                if (route.Where(x => x.PageURL == rt.PageURL).Count() > 0)
                    continue;

                var new_route_list = route.ToList();
                new_route_list.Add(rt);

                var decider = RecurseRoute(cts, ref lockable, ref best_score, new_route_list, url);

                if (decider != null)
                    winning_route = decider;
            }

            return winning_route;
        }

        public ILocator Locate(string selector, LocatorLocatorOptions? locatorLocatorOptions = null)
        {
            var tmplocator = Locator.Locator(selector, locatorLocatorOptions);
            if (tmplocator.CountAsync().Result == 0)
            {
                PageLocatorOptions? pageLocatorOptions = null;
                if (locatorLocatorOptions != null)
                {
                    pageLocatorOptions = new PageLocatorOptions();
                    pageLocatorOptions.Has = locatorLocatorOptions.Has;
                    pageLocatorOptions.HasNot = locatorLocatorOptions.HasNot;
                    pageLocatorOptions.HasNotText = locatorLocatorOptions.HasNotText;
                    pageLocatorOptions.HasNotTextRegex = locatorLocatorOptions.HasNotTextRegex;
                    pageLocatorOptions.HasTextRegex = locatorLocatorOptions.HasTextRegex;
                    pageLocatorOptions.HasText = locatorLocatorOptions.HasText;
                }
                tmplocator = Page.Locator(selector, pageLocatorOptions);
            }

            _locator = tmplocator;
            return _locator;
        }

        public ILocator Locate(AriaRole role, LocatorGetByRoleOptions? options = null)
        {
            var tmplocator = Locator.GetByRole(AriaRole.Textbox, options);
            if (tmplocator.CountAsync().Result == 0)
            {
                PageGetByRoleOptions? pageGetByRoleOptions = null;
                if (options != null)
                {
                    pageGetByRoleOptions = new PageGetByRoleOptions();
                    pageGetByRoleOptions.Checked = options.Checked;
                    pageGetByRoleOptions.Expanded = options.Expanded;
                    pageGetByRoleOptions.Pressed = options.Pressed;
                    pageGetByRoleOptions.Selected = options.Selected;
                    pageGetByRoleOptions.Level = options.Level;
                    pageGetByRoleOptions.Disabled = options.Disabled;
                    pageGetByRoleOptions.Exact = options.Exact;
                    pageGetByRoleOptions.IncludeHidden = options.IncludeHidden;
                    pageGetByRoleOptions.Name = options.Name;
                    pageGetByRoleOptions.NameRegex = options.NameRegex;
                    pageGetByRoleOptions.NameString = options.NameString;
                }
                tmplocator = Page.GetByRole(role, pageGetByRoleOptions);
            }
            _locator = tmplocator;
            return _locator;
        }

        public IPageAssertions CurrentPage() => Assertions.Expect(Page);

        public ILocatorAssertions CurrentLocator() => Assertions.Expect(Locator.First);
        public Task PauseAsync() => Page.PauseAsync();

        public async Task<AxeResult> CheckAODA(bool onLocatorOnly = false)
        {

            var runOnly = new RunOnlyOptions();
            runOnly.Type = "tag";
            runOnly.Values.Add("wcag2a");
            runOnly.Values.Add("wcag2aa");

            AxeResult? result;
            if (onLocatorOnly)
                result = await Locator.RunAxe(new AxeRunOptions() { RunOnly = runOnly });
            else
                result = await Page.RunAxe(new AxeRunOptions() { RunOnly = runOnly });
            return result;
        }

        public void Dispose()
        {
            if (_browserContext != null)
            {
                var task = _browserContext.Result.CloseAsync();
                task.Wait();
            }

        }

        public event EventHandler<IDownload> OnDownload { add { Page.Download += value; } remove { Page.Download -= value; } }

    }
}
