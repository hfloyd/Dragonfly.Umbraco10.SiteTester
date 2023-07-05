namespace Dragonfly.UmbracoSiteTester
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Dragonfly.UmbracoSiteTester.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Umbraco.Cms.Core.Cache;
    using Umbraco.Cms.Core.Hosting;
    using Umbraco.Cms.Core.Models;
    using Umbraco.Cms.Core.Services;
    using Umbraco.Cms.Web.Common;
    using Umbraco.Cms.Web.Common.UmbracoContext;
    using Umbraco.Extensions;


    public class SiteTesterService
    {
        #region CTOR/DI

        private readonly IHostingEnvironment _HostingEnvironment;
        private readonly ILogger _logger;
        private readonly AppCaches _appCaches;
        private readonly UmbracoContext _umbracoContext;
        private readonly DependencyLoader _Dependencies;
        private readonly Dragonfly.NetHelperServices.FileHelperService _FileHelperService;
        private readonly HttpContext _context;
        private readonly UmbracoHelper _umbracoHelper;
        private readonly ServiceContext _services;
        private readonly IConfiguration _appSettingsConfig;

        public SiteTesterService(DependencyLoader dependencies,
            ILogger<SiteTesterService> logger,
            HttpRenderingService HttpRenderingService)
        {
            //Services
            _Dependencies = dependencies;
            _HostingEnvironment = dependencies.HostingEnvironment;
            _umbracoHelper = dependencies.UmbHelper;
            _context = dependencies.Context;
            _logger = logger;
            _services = dependencies.Services;
            _appSettingsConfig = dependencies.AppSettingsConfig;

            _httpRenderService = HttpRenderingService;
            _FileHelperService = dependencies.DragonflyFileHelperService;

            //this._LinkTests = new List<LinkTest>();

            //Config Data
            _config = GetAppDataConfig();
            this.InternalDomains = _config.InternalDomains;
            this.LinkTypesToTest = _config.LinkTypesToTest;
            this.PageExtensions = _config.PageExtensions;
            this.AlsoTestOnPageLinks = _config.AlsoTestOnPageLinks;

            var thisDomain = $"{_context.Request.Scheme}{_context.Request.Host.ToString()}";
            var finalDomain = _config.HttpHost != "" ? _config.HttpHost : thisDomain;
            var httpUrl = _config.HttpUrl;
            var timeout = _config.HttpTimeout;
            _httpRenderService.SetProps(finalDomain, httpUrl, _config.IgnoreSslErrors, timeout);
            //var testerService = new SiteTesterService(httpService);

            var configTz = _config.LocalTimezone;
            var localTimezone = configTz != "" ? configTz : TimeZoneInfo.Local.Id;
            this.LocalTimezone = localTimezone;

            var configErrors = _config.InlineErrorStrings;
            this.AddErrorTextMatchStrings(configErrors);

            var storeHtml = _config.StoreRenderedHtml;
            this.StoreRenderedHtml = storeHtml;

        }

        #endregion


        #region Private Vars

        private HttpRenderingService _httpRenderService;

        private Config _config;

        private List<RenderingResult> _ResultList = new List<RenderingResult>();
        private List<string> _ErrorText = new List<string>();

        #endregion

        public IEnumerable<string> InternalDomains { get; set; }
        public IEnumerable<string> LinkTypesToTest { get; set; }
        public IEnumerable<string> PageExtensions { get; set; }
        public bool AlsoTestOnPageLinks { get; set; }

        public IEnumerable<string> ErrorTextToMatch => _ErrorText;
        public bool StoreRenderedHtml { get; set; }
        public DateTime TestStartTimestamp { get; internal set; }
        public DateTime TestEndTimestamp { get; internal set; }
        public string LocalTimezone { get; set; }
        public int TestingStartNode { get; set; }

        //public IEnumerable<RenderingResult> Results => _ResultList;

        internal string DataPath()
        {
            return _config.DataStoragePath;
            //return "~/App_Data/DragonflySiteTester/";
        }

        internal string PluginPath()
        {
            return _config.GetAppPluginsPath();
            //return "~/App_Plugins/Dragonfly.SiteTester/";
        }

        #region Public Functions
        public Config GetAppDataConfig()
        {
            var options = new Config();
            _appSettingsConfig.GetSection(Config.ConfigSectionName).Bind(options);

            return options;
        }

        public void AddErrorTextMatchString(string Text)
        {
            _ErrorText.Add(Text);
        }

        public void AddErrorTextMatchStrings(IEnumerable<string> Text)
        {
            _ErrorText.AddRange(Text);
        }

        public TestResultSet GetResultSet()
        {
            var trs = new TestResultSet();
            trs.TestStartTimestamp = this.TestStartTimestamp;
            trs.TestEndTimestamp = this.TestEndTimestamp;
            trs.LocalTimeZone = this.LocalTimezone;
            trs.SiteDomain = this._httpRenderService.HttpHost;
            trs.HasStoredHtml = this.StoreRenderedHtml;
            trs.HasLinkTestResults = this.AlsoTestOnPageLinks;
            trs.Results = this._ResultList;
            trs.StartNode = this.TestingStartNode;


            trs.Summary = SummarizeNodeTestResults(this._ResultList);

            return trs;
        }

        #endregion


        #region Test Nodes

        public void TestAllPublishedNodes()
        {
            _logger.LogInformation("Dragonfly.SiteTesterService TestAllPublishedNodes Started ...");
            this.TestStartTimestamp = DateTime.Now;

            var rootContent = _services.ContentService.GetRootContent();
            if (rootContent != null)
            {
                foreach (var c in rootContent)
                {
                    _logger.LogDebug("{Function} : {Message} : {NodeId}", "Dragonfly.SiteTesterService.TestAllPublishedNodes", "Starting RecursiveTestNodes()", c.Id);
                    RecursiveTestNodes(c);
                }
            }
            else
            {
                _logger.LogWarning("{Function} : {Message}", "Dragonfly.SiteTesterService.TestAllPublishedNodes", "ROOT CONTENT IS NULL");
            }

            this.TestEndTimestamp = DateTime.Now;
            _logger.LogInformation("... Dragonfly.SiteTesterService TestAllPublishedNodes Completed");
        }

        public void TestDescendantNodes(int StartNode)
        {
            _logger.LogInformation($"Dragonfly.SiteTesterService TestDescendantNodes for #{StartNode} Started ...");
            this.TestStartTimestamp = DateTime.Now;

            this.TestingStartNode = StartNode;

            var rootContent = _services.ContentService.GetById(StartNode);
            if (rootContent != null)
            {
                RecursiveTestNodes(rootContent);
            }

            this.TestEndTimestamp = DateTime.Now;
            _logger.LogInformation($"... Dragonfly.SiteTesterService TestDescendantNodes for #{StartNode}  Completed");
        }

        protected void RecursiveTestNodes(IContent Content)
        {
            if (Content != null && !Content.Trashed)
            {
                _logger.LogDebug("{Function} : {Message} : {NodeId}", "Dragonfly.SiteTesterService.TestAllPublishedNodes", "Starting TestNode()", Content.Id);
                TestNode((Content)Content);

                if (_services.ContentService.HasChildren(Content.Id))
                {
                    var countChildren = _services.ContentService.CountChildren(Content.Id);
                    long xTotalRecs;
                    var allChildren = _services.ContentService.GetPagedChildren(Content.Id, 0, countChildren, out xTotalRecs);

                    foreach (var child in allChildren)
                    {
                        RecursiveTestNodes(child);
                    }
                }
            }
        }
        protected void TestNode(IContent Content)
        {
            if (Content != null && Content.Id > 0)
            {
                var testNode = true;

                //Set basic node info
                var result = new RenderingResult();
                result.NodeId = Content.Id;
                result.NodeUdi = Content.GetUdi();
                result.NodeName = Content.Name;
                result.ContentTypeAlias = Content.ContentType.Alias;

                //check for excluded Type
                if (_config.ContentTypesToExclude.Any())
                {
                    if (_config.ContentTypesToExclude.Contains(Content.ContentType.Alias))
                    {
                        result.Result = ResultType.NotTestedExcludedByConfig;
                        testNode = false;
                    }
                }

                //check published
                if (!Content.Published)
                {
                    result.Result = ResultType.NotTestedUnPublished;
                    result.Url = "[Unpublished]";
                    testNode = false;
                }

                // check if document is protected
                //var path = Content.Path;
                //if (!_umbracoHelper. (path))
                //{
                //	result.Result = ResultType.NotTestedProtected;
                //}

                //Get URL
                try
                {
                    result.Url = _umbracoHelper.Content(Content.Id).Url();
                }
                catch (Exception e)
                {
                    testNode = false;
                    _logger.LogDebug(e, "TestNode: Unable to get URL for Node #{NodeId}", Content.Id);
                }


                //Rendering Test
                if (testNode)
                {
                    var html = "";
                    try
                    {
                        var renderResult = _httpRenderService.HttpRenderNode(Content, out html);

                        //Check for SSL Issue
                        if (html.Contains("HTTP RENDERING WebException"))
                        {
                            result.StatusCodeMessage = html;
                        }
                        result.ContentErrorMatches = CheckHtmlForErrors(html);

                        //Store HTML?
                        if (StoreRenderedHtml)
                        {
                            result.RenderedOutput = html;
                        }
                        else
                        {
                            result.RenderedOutput = "[NOT STORED]";
                        }

                        //Gather Links?
                        if (_config.AlsoTestOnPageLinks)
                        {
                            result.OnPageLinks = GatherOnPageLinks(html, Content.Id);
                        }

                        result.StatusCode = renderResult.StatusCode;
                        result.StatusCodeMessage = renderResult.StatusCodeMessage;
                        result.Result = GetResultTypeForStatusCode(renderResult.StatusCode);

                        if (result.Result == ResultType.Ok && result.ContentErrorMatches.Any())
                        {
                            result.Result = ResultType.CheckPossibleInlineError;
                        }

                        if (result.StatusCode == HttpStatusCode.InternalServerError || result.StatusCode == 0)
                        {
                            result.SetException(renderResult.ErrorException);
                        }

                        if (this.AlsoTestOnPageLinks)
                        {
                            var links = TestOnPageLinks(html, Content.Id);
                            result.OnPageLinks = links;
                        }
                        else
                        {
                            result.OnPageLinks = new LinksSet();
                        }

                    }
                    catch (Exception e)
                    {
                        result.RenderedOutput = html;
                        result.SetException(e);

                        if (e.Message.Contains("404"))
                        {
                            result.Result = ResultType.Error404;
                        }
                        else
                        {
                            result.Result = ResultType.ErrorRequestException;
                            result.SetException(e.InnerException);
                        }
                    }
                }

                _ResultList.Add(result);
            }
        }

        #endregion

        #region Test Links

        private LinksSet GatherOnPageLinks(string Html, int OnNodeId)
        {
            var linkSet = new LinksSet();
            var tests = new List<LinkTest>();

            if (!string.IsNullOrEmpty(Html))
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(Html);

                //Gather Asset Links
                var linkTags = doc.DocumentNode.Descendants("link");
                if (linkTags.Any())
                {
                    foreach (var node in linkTags)
                    {
                        var url = node.GetAttributeValue("href", null);

                        var testItem = new LinkTest();
                        testItem.OnNode = OnNodeId;
                        testItem.Url = url;
                        testItem.Type = LinkTest.LinkType.Asset;
                        testItem.DoTest = LinkShouldBeTested(LinkTest.LinkType.Asset);
                        tests.Add(testItem);
                    }
                }

                //Gather Page Links
                var linkedPages = doc.DocumentNode.Descendants("a")
                    .Select(a => a.GetAttributeValue("href", null))
                    .Where(u => !String.IsNullOrEmpty(u)).ToList();

                if (linkedPages.Any())
                {
                    foreach (var href in linkedPages)
                    {
                        if (IsValidLink(href))
                        {
                            var testItem = new LinkTest();
                            testItem.OnNode = OnNodeId;
                            testItem.Url = href;
                            testItem.Type = GetLinkType(href);
                            testItem.DoTest = LinkShouldBeTested(testItem.Type);
                            tests.Add(testItem);
                        }
                    }
                }
            }

            linkSet.TestsPerformed = false;
            linkSet.LinkTests = tests;

            linkSet.TypesTested = new List<string>();

            linkSet.LinkTypesSummary = SummarizeLinkTypes(tests);
            //linkSet.DoTestSummary = SummarizeLinkDoTests(tests);
            //linkSet.ResultsSummary = SummarizeLinkTestResults(tests);

            return linkSet;
        }


        private LinksSet TestOnPageLinks(string Html, int OnNodeId)
        {
            var linkSet = new LinksSet();
            var tests = new List<LinkTest>();

            if (!string.IsNullOrEmpty(Html))
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(Html);

                //Gather Asset Links
                var linkTags = doc.DocumentNode.Descendants("link");
                if (linkTags.Any())
                {
                    foreach (var node in linkTags)
                    {
                        var url = node.GetAttributeValue("href", null);

                        var testItem = new LinkTest();
                        testItem.OnNode = OnNodeId;
                        testItem.Url = url;
                        testItem.Type = LinkTest.LinkType.Asset;
                        testItem.DoTest = LinkShouldBeTested(LinkTest.LinkType.Asset);
                        tests.Add(testItem);
                    }
                }

                //Gather Page Links
                var linkedPages = doc.DocumentNode.Descendants("a")
                    .Select(a => a.GetAttributeValue("href", null))
                    .Where(u => !String.IsNullOrEmpty(u)).ToList();

                if (linkedPages.Any())
                {
                    foreach (var href in linkedPages)
                    {
                        if (IsValidLink(href))
                        {
                            var testItem = new LinkTest();
                            testItem.OnNode = OnNodeId;
                            testItem.Url = href;
                            testItem.Type = GetLinkType(href);
                            testItem.DoTest = LinkShouldBeTested(testItem.Type);
                            tests.Add(testItem);
                        }
                    }
                }
            }

            //Get distinct list of urls to test
            var allUrls = tests.Where(n => n.DoTest).Select(n => n.Url);
            var distinctUrls = allUrls.Distinct();

            //Test each url
            foreach (var url in distinctUrls)
            {
                var checkContents = UrlIsInternal(url) && !UrlIsFile(url);
                var result = TestUrl(url, checkContents);

                //Update any matching with the result
                var matchingTests = tests.Where(n => n.Url == url);
                foreach (var test in matchingTests)
                {
                    test.Result = result;
                }
            }

            linkSet.LinkTests = tests;

            linkSet.TypesTested = LinkTypesToTest;

            linkSet.LinkTypesSummary = SummarizeLinkTypes(tests);
            linkSet.DoTestSummary = SummarizeLinkDoTests(tests);
            linkSet.ResultsSummary = SummarizeLinkTestResults(tests);

            return linkSet;
        }


        private RenderingResult TestUrl(string Url, bool CheckForInternalErrors)
        {
            var isExternal = !UrlIsInternal(Url);
            var result = new RenderingResult();
            //result.NodeId = 0;
            //result.NodeUdi = null;
            //result.NodeName = "";
            //result.ContentTypeAlias = "";
            result.Url = Url;

            //Remove trailing slashes for urls
            var testUrl = Url.TrimEnd('/');

            var html = "";
            try
            {
                var renderResult = _httpRenderService.HttpRenderUrl(testUrl, isExternal, out html);

                if (CheckForInternalErrors)
                {
                    result.ContentErrorMatches = CheckHtmlForErrors(html);
                }

                if (StoreRenderedHtml)
                {
                    result.RenderedOutput = html;
                }
                else
                {
                    result.RenderedOutput = "[NOT STORED]";
                }

                result.StatusCode = renderResult.StatusCode;
                result.StatusCodeMessage = renderResult.StatusCodeMessage;
                result.Result = GetResultTypeForStatusCode(renderResult.StatusCode);

                if (result.ContentErrorMatches != null)
                {
                    if (result.Result == ResultType.Ok && result.ContentErrorMatches.Any())
                    {
                        result.Result = ResultType.CheckPossibleInlineError;
                    }
                }

                if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    result.SetException(renderResult.ErrorException);
                }

            }
            catch (Exception e)
            {
                result.RenderedOutput = html;
                result.SetException(e);

                if (e.Message.Contains("404"))
                {
                    result.Result = ResultType.Error404;
                }
                else
                {
                    result.Result = ResultType.ErrorRequestException;
                    result.SetException(e.InnerException);
                }
            }

            return result;
        }


        #endregion


        #region Private Methods

        private ResultType GetResultTypeForStatusCode(HttpStatusCode StatusCode)
        {
            switch (StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return ResultType.Ok;
                    break;

                case System.Net.HttpStatusCode.InternalServerError:
                    return ResultType.Error500;
                    break;
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    return ResultType.Error500;
                    break;

                case System.Net.HttpStatusCode.NotFound:
                    return ResultType.Error404;
                    break;

                case System.Net.HttpStatusCode.TemporaryRedirect:
                    return ResultType.Redirected;
                    break;
                case System.Net.HttpStatusCode.MovedPermanently:
                    return ResultType.Redirected;
                    break;

                case System.Net.HttpStatusCode.BadRequest:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.PaymentRequired:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.NoContent:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.MethodNotAllowed:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.NotAcceptable:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.ProxyAuthenticationRequired:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.BadGateway:
                    return ResultType.ErrorHttpOther;
                    break;
                case System.Net.HttpStatusCode.GatewayTimeout:
                    return ResultType.ErrorHttpOther;
                    break;

                default:
                    return ResultType.Unknown;
                    break;
            }
        }

        private IEnumerable<string> CheckHtmlForErrors(string Html)
        {
            var errorMatches = new List<string>();
            if (this._ErrorText.Any())
            {
                foreach (var errorText in _ErrorText)
                {
                    var hasMatch = Html.Contains(errorText);
                    if (hasMatch)
                    { errorMatches.Add(errorText); }
                }
            }

            return errorMatches;
        }


        #endregion

        #region Summary Functions
        public Dictionary<string, long> SummarizeNodeTestResults(IEnumerable<RenderingResult> RenderingResults)
        {
            var summaryGroups = this._ResultList.GroupBy(n => n.Result);
            var summaryDict = new Dictionary<string, long>();
            foreach (var grp in summaryGroups)
            {
                summaryDict.Add(grp.Key.ToString(), grp.Count());
            }

            return summaryDict;
        }

        public Dictionary<bool, long> SummarizeLinkDoTests(IEnumerable<LinkTest> LinkTests)
        {
            var summaryGroups = LinkTests.GroupBy(n => n.DoTest);
            var summaryDict = new Dictionary<bool, long>();
            foreach (var grp in summaryGroups)
            {
                summaryDict.Add(grp.Key, grp.Count());
            }

            return summaryDict;
        }

        public Dictionary<string, long> SummarizeLinkTypes(IEnumerable<LinkTest> LinkTests)
        {
            var summaryGroups = LinkTests.GroupBy(n => n.Type);
            var summaryDict = new Dictionary<string, long>();
            foreach (var grp in summaryGroups)
            {
                summaryDict.Add(grp.Key.ToString(), grp.Count());
            }

            return summaryDict;
        }

        public Dictionary<string, long> SummarizeLinkTestResults(List<LinkTest> LinkTests)
        {
            var testsWithResults = LinkTests.Where(n => n.Result != null);
            var summaryGroups = testsWithResults.GroupBy(n => n.Result.Result);
            var summaryDict = new Dictionary<string, long>();
            foreach (var grp in summaryGroups)
            {
                summaryDict.Add(grp.Key.ToString(), grp.Count());
            }

            return summaryDict;
        }

        #endregion





        #region Link Test Helpers

        private bool LinkShouldBeTested(LinkTest.LinkType LinkType)
        {
            var testTypes = LinkTypesToTest.ToList();
            var thisType = LinkType.ToString();

            if (testTypes.Contains(thisType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsValidLink(string Url)
        {
            if (Url.StartsWith("#"))
            {
                return false;
            }

            return true;
        }

        private LinkTest.LinkType GetLinkType(string Url)
        {
            //Umbraco
            if (Url.StartsWith("umb://document"))
            {
                return LinkTest.LinkType.UmbracoDoc;
            }
            if (Url.StartsWith("umb://media"))
            {
                return LinkTest.LinkType.UmbracoMedia;
            }

            //Mail
            if (Url.StartsWith("mailto"))
            {
                return LinkTest.LinkType.Email;
            }

            var isInternal = UrlIsInternal(Url);

            //Internal
            if (isInternal)
            {
                if (Url.Contains("."))
                {
                    if (UrlIsFile(Url))
                    {
                        return LinkTest.LinkType.InternalFile;
                    }
                }

                //InternalPage
                if (Url.Contains("?"))
                {
                    return LinkTest.LinkType.InternalPageWithQueryString;
                }

                return LinkTest.LinkType.InternalPage;
            }

            //External
            if (Url.StartsWith("http") || Url.StartsWith("//"))
            {
                return LinkTest.LinkType.External;
            }

            //Unknown 
            return LinkTest.LinkType.Unknown;
        }

        private bool UrlIsInternal(string Url)
        {
            if (Url.StartsWith("~") || (Url.StartsWith("/") && !Url.StartsWith("//")))
            {
                return true;
            }

            if (HasInternalDomain(Url))
            {
                return true;
            }

            //No match - External
            return false;
        }


        private bool HasInternalDomain(string Url)
        {
            var domains = InternalDomains.ToList();
            foreach (var domain in domains)
            {
                if (Url.Contains(domain))
                {
                    return true;
                }
            }
            return false;
        }

        private bool UrlIsFile(string Url)
        {
            if (Url.Contains("."))
            {
                //Remove domain, if present
                var strippedUrl = Url;
                if (HasInternalDomain(Url))
                {
                    strippedUrl = RemoveInternalDomains(Url);
                }

                //Does it still have any "."?
                if (strippedUrl.Contains("."))
                {
                    var split = strippedUrl.Split('.').ToList();
                    var lastExtension = split.Last();

                    var pageExtensions = PageExtensions.ToList();
                    foreach (var ext in pageExtensions)
                    {
                        if (lastExtension == ext)
                        {
                            return false;
                        }
                    }

                    //No match
                    return true;
                }
                else
                {
                    //No '.'
                    return false;
                }
            }
            else
            {
                //No '.'
                return false;
            }
        }

        private string RemoveInternalDomains(string Url)
        {
            var strippedUrl = Url;
            var domains = InternalDomains.ToList();
            foreach (var domain in domains)
            {
                if (Url.Contains(domain))
                {
                    strippedUrl = strippedUrl.Replace(domain, "/");
                    strippedUrl = strippedUrl.Replace(".//", "/"); //Clear up subdomains
                }
            }

            return strippedUrl;
        }

        #endregion



    }
}
