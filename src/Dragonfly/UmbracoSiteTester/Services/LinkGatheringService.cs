namespace Dragonfly.UmbracoSiteTester.Services
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using Dragonfly.Umbraco8SiteTester.Models;
    using Dragonfly.Umbraco8SiteTester.Utilities;
    using HtmlAgilityPack;

    class LinkGatheringService
    {
        private TestResultSet _ResultSet;

        private List<LinkTest> _LinkTests;

        private string _ResultSetFilename;

        private IEnumerable<string> _InternalDomains;

        private IEnumerable<string> _LinkTypesToTest;
        private IEnumerable<string> _PageExtensions;

        private HttpRenderingService httpRenderService;

        //public IEnumerable<LinkTest> LinkTests
        //{
        //    get { return _LinkTests; }
        //}



        public LinkGatheringService(HttpRenderingService HttpRenderingService, string ResultSetFilename)
        {
            //this._LinkTests = new List<LinkTest>();
            //this._ResultSetFilename = ResultSetFilename;
            //this.httpRenderService = HttpRenderingService;

            //var config = Config.GetConfig();
            //this._InternalDomains = config.InternalDomains;
            //this._LinkTypesToTest = config.LinkTypesToTest;
            //this._PageExtensions = config.PageExtensions;

            //TestResultSet resultSet;
            //var resultsMsg = FilesIO.ReadResultSet(ResultSetFilename, out resultSet);
            //this._ResultSet = resultSet;
        }

        

        public void GatherLinks(bool DoTests = true)
        {
            foreach (var result in _ResultSet.Results)
            {
                if (!string.IsNullOrEmpty(result.RenderedOutput))
                {
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(result.RenderedOutput);

                    var linkTags = doc.DocumentNode.Descendants("link");
                    if (linkTags.Any())
                    {
                        foreach (var node in linkTags)
                        {
                            var url = node.GetAttributeValue("href", null);

                            var testItem = new LinkTest();
                            testItem.OnNode = result.NodeId;
                            testItem.Url = url;
                            testItem.Type = LinkTest.LinkType.Asset;
                            testItem.DoTest = LinkShouldBeTested(LinkTest.LinkType.Asset);
                            _LinkTests.Add(testItem);
                        }
                    }

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
                                testItem.OnNode = result.NodeId;
                                testItem.Url = href;
                                testItem.Type = GetLinkType(href);
                                testItem.DoTest = LinkShouldBeTested(testItem.Type);
                                _LinkTests.Add(testItem);
                            }
                        }
                    }
                }
            }
            this.LinksGathered = true;

            if (DoTests)
            {
                TestLinks();
                this.LinksTested = true;
            }
        }

        public void TestLinks()
        {
            if (!this.LinksGathered)
            {
                GatherLinks(true);
            }
            else
            {
                //Get distinct list of urls to test
                var allUrls = _LinkTests.Where(n => n.DoTest).Select(n=> n.Url);
                var distinctUrls = allUrls.Distinct();

                //Test each url
                foreach (var url in distinctUrls)
                {
                    var result = TestUrl(url);

                    //Update any matching with the result
                    var matchingTests = _LinkTests.Where(n => n.Url == url);
                    foreach (var test in matchingTests)
                    {
                        test.Result = result;
                    }
                }
            }

        }

      



    }
}
