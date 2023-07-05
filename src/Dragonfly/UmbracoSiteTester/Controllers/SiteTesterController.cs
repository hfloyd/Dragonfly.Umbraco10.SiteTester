#pragma warning disable 1591
namespace Dragonfly.UmbracoSiteTester
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Dragonfly.NetHelperServices;
    using Dragonfly.NetModels;
    using Dragonfly.UmbracoSiteTester.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Umbraco.Cms.Web.BackOffice.Controllers;
    using Umbraco.Cms.Web.Common;
    using Umbraco.Cms.Web.Common.Attributes;

    //  /umbraco/backoffice/Dragonfly/SiteTester/
    [PluginController("Dragonfly")]
    [IsBackOffice]
    public class SiteTesterController : UmbracoAuthorizedApiController
    {
        #region CTOR/DI

        private readonly DependencyLoader _dependencies;
        private readonly ILogger<SiteTesterController> _logger;
        private readonly SiteTesterService _siteTesterService;
        private readonly HtmlSearchService _htmlSearchService;
        private readonly IViewRenderService _viewRenderService;
        private readonly UmbracoHelper _umbracoHelper;
        private readonly FilesIO _filesIo;

        public SiteTesterController(DependencyLoader dependencies,
                    ILogger<SiteTesterController> logger,
                    SiteTesterService siteTesterService,
                    FilesIO filesIo,
                    HtmlSearchService htmlSearchService,
                    IViewRenderService viewRenderService)
        {
            _logger = logger;
            _dependencies = dependencies;
            _siteTesterService = siteTesterService;
            _htmlSearchService = htmlSearchService;
            _viewRenderService = viewRenderService;
            _umbracoHelper = dependencies.UmbHelper;

            _testerConfig = _siteTesterService.GetAppDataConfig();
            _filesIo = filesIo;
            _filesIo.SetConfig(_testerConfig);
        }

        #endregion

        #region Private Vars

        private Config _testerConfig;
        private string RazorFilesPath()
        {
            return _siteTesterService.PluginPath() + "RazorViews/";
        }

        #endregion

        #region Actions (returns JSON)

        /// /umbraco/backoffice/Dragonfly/SiteTester/TestAllNodesRaw
        [HttpGet]
        public IActionResult TestAllNodesRaw()
        {
            //GET DATA TO DISPLAY
            _siteTesterService.TestAllPublishedNodes();
            var results = _siteTesterService.GetResultSet();

            string json = JsonConvert.SerializeObject(results);

            var result = new HttpResponseMessage()
            {
                Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                )
            };
            return new HttpResponseMessageResult(result);
        }

        #endregion

        #region Views (returns HTML)

        //  /umbraco/backoffice/Dragonfly/SiteTester
        [HttpGet]
        public IActionResult Index()
        {
            return Start();
        }

        /// /umbraco/backoffice/Dragonfly/SiteTester/Start
        [HttpGet]
        public IActionResult Start()
        {
            //Setup
            var pvPath = RazorFilesPath() + "Start.cshtml";

            //GET DATA TO DISPLAY
            var returnStatusMsg = new StatusMessage(true); //assume success
            var specialMessage = "";
            var specialMessageClass = "bg-info";

            IEnumerable<FileInfo> filesList;
            var filesMsg = _filesIo.GetListOfFiles(out filesList);

            //UPDATE STATUS MSG
            returnStatusMsg.InnerStatuses.Add(filesMsg);
            returnStatusMsg.Success = filesMsg.Success;
            if (!filesList.Any())
            {
                returnStatusMsg.Success = false;
                returnStatusMsg.Message = "There are no Test Result Sets available.";
                returnStatusMsg.Code = "NoResultSetFiles";
            }

            var model = returnStatusMsg;

            //VIEW DATA 
            var viewData = new Dictionary<string, object>();
            viewData.Add("FilesList", filesList);
            viewData.Add("SpecialMessage", specialMessage);
            viewData.Add("SpecialMessageClass", specialMessageClass);
            viewData.Add("DeleteOptions", FilesIO.GetFilesOptions());

            //RENDER
            var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
            var displayHtml = htmlTask.Result;

            //RETURN AS HTML
            var result = new HttpResponseMessage()
            {
                Content = new StringContent(
                   displayHtml,
                   Encoding.UTF8,
                   "text/html"
               )
            };

            return new HttpResponseMessageResult(result);
        }

        /// /umbraco/backoffice/Dragonfly/SiteTester/DeleteResultSets?Option=None
        [HttpGet]
        public IActionResult DeleteResultSets(FilesOption Option)
        {
            //Setup
            var pvPath = RazorFilesPath() + "Start.cshtml";

            //GET DATA TO DISPLAY
            var returnStatusMsg = new StatusMessage(true); //assume success
            var specialMessage = "";
            var specialMessageClass = "bg-info";

            //Setup
            //var testerService = SetupServices();
            //testerService.TestAllPublishedNodes();
            //var results = testerService.GetResultSet();

            //DO FILES DELETION
            var deleteMsg = new StatusMessage();
            bool validOption = false;
            switch (Option)
            {
                case FilesOption.All:
                    deleteMsg = _filesIo.DeleteFiles(Option);
                    validOption = true;
                    break;

                case FilesOption.ExceptLastOne:
                    deleteMsg = _filesIo.DeleteFiles(Option);
                    validOption = true;
                    break;

                case FilesOption.None:
                    //Ignore
                    break;

                default:
                    specialMessage = $"{Option.ToString()} is not a valid File deleting option.";
                    specialMessageClass = "bg-danger";
                    break;
            }

            if (validOption)
            {
                returnStatusMsg = deleteMsg;
                var cliMsg = deleteMsg.InnerStatuses.First();
                if (deleteMsg.Success)
                {
                    specialMessage = $"Files deleted successfully.<br/>{cliMsg.Message}";
                    specialMessageClass = "bg-success";
                }
                else
                {
                    specialMessage = $"Error while deleting files.<br/>{cliMsg.Message}<br/>{cliMsg.GetRelatedException().Message}";
                    specialMessageClass = "bg-danger";
                }
            }

            //GET DATA TO DISPLAY

            IEnumerable<FileInfo> filesList;
            var filesMsg = _filesIo.GetListOfFiles(out filesList);

            //UPDATE STATUS MSG
            returnStatusMsg.InnerStatuses.Add(filesMsg);
            returnStatusMsg.Success = filesMsg.Success;
            if (!filesList.Any())
            {
                returnStatusMsg.Success = false;
                returnStatusMsg.Message = "There are no Test Result Sets available.";
                returnStatusMsg.Code = "NoResultSetFiles";
            }

            var model = returnStatusMsg;

            //VIEW DATA 
            var viewData = new Dictionary<string, object>();
            viewData.Add("FilesList", filesList);
            viewData.Add("SpecialMessage", specialMessage);
            viewData.Add("SpecialMessageClass", specialMessageClass);
            viewData.Add("DeleteOptions", FilesIO.GetFilesOptions());

            //RENDER
            var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
            var displayHtml = htmlTask.Result;

            //RETURN AS HTML
            var result = new HttpResponseMessage()
            {
                Content = new StringContent(
                    displayHtml,
                    Encoding.UTF8,
                    "text/html"
                )
            };

            return new HttpResponseMessageResult(result);
        }

        /// /umbraco/backoffice/Dragonfly/SiteTester/TestAllNodesAndSave
        [HttpGet]
        public IActionResult TestAllNodesAndSave()
        {
            //Setup
            var pvPath = RazorFilesPath() + "SaveResult.cshtml";

            //GET DATA TO DISPLAY
            var returnStatusMsg = new StatusMessage(true); //assume success

            //Setup
            _siteTesterService.TestAllPublishedNodes();
            var results = _siteTesterService.GetResultSet();

            //Save Results
            returnStatusMsg = _filesIo.SaveResultSet(results);
            var filename = "";
            if (returnStatusMsg.Success)
            {
                filename = returnStatusMsg.ObjectName.Replace(_siteTesterService.DataPath(), "");
            }

            var model = returnStatusMsg;

            //VIEW DATA 
            var viewData = new Dictionary<string, object>();
            viewData.Add("Filename", filename);
            viewData.Add("Success", returnStatusMsg.Success);

            //RENDER
            var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
            var displayHtml = htmlTask.Result;

            //RETURN AS HTML
            var result = new HttpResponseMessage()
            {
                Content = new StringContent(
                    displayHtml,
                    Encoding.UTF8,
                    "text/html"
                )
            };

            return new HttpResponseMessageResult(result);
        }

        /// /umbraco/backoffice/Dragonfly/SiteTester/TestDescendantNodesAndSave?StartNode=xxx
        [HttpGet]
        public IActionResult TestDescendantNodesAndSave(int StartNode)
        {
            //Setup
            var pvPath = RazorFilesPath() + "SaveResult.cshtml";

            //GET DATA TO DISPLAY
            var returnStatusMsg = new StatusMessage(true); //assume success

            //VIEW DATA 
            var viewData = new Dictionary<string, object>();

            if (StartNode == 0)
            {
                //Show Options for selecting nodes
                pvPath = RazorFilesPath() + "SelectNode.cshtml";

                //GET DATA TO DISPLAY
                var allRootNodes = _umbracoHelper.ContentAtRoot();

                //VIEW DATA 
                viewData.Add("model", allRootNodes);
                //viewData.Add("Filename", "");
                //viewData.Add("Success", returnStatusMsg.Success);
            }
            else
            {
                //Do Test
                _siteTesterService.TestDescendantNodes(StartNode);
                var results = _siteTesterService.GetResultSet();

                //Save Results
                returnStatusMsg = _filesIo.SaveResultSet(results);
                var filename = returnStatusMsg.ObjectName.Replace(_siteTesterService.DataPath(), "");

                //VIEW DATA 
                viewData.Add("model", returnStatusMsg);
                viewData.Add("Filename", filename);
                viewData.Add("Success", returnStatusMsg.Success);
            }

            var model = viewData["model"];

            //RENDER
            var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
            var displayHtml = htmlTask.Result;

            //RETURN AS HTML
            var result = new HttpResponseMessage()
            {
                Content = new StringContent(
                    displayHtml,
                    Encoding.UTF8,
                    "text/html"
                )
            };

            return new HttpResponseMessageResult(result);
        }


        /// /umbraco/backoffice/Dragonfly/SiteTester/ViewResults?ForTimestamp=xxx
        /// /umbraco/backoffice/Dragonfly/SiteTester/ViewResults?ForTimestamp=xxx&ForDomain=xxx
        // [HttpGet]
        //public HttpResponseMessage ViewResults(string ForTimestamp, string ForDomain = "")
        //{
        //    var returnSB = new StringBuilder();
        //    var returnStatusMsg = new StatusMessage(true); //assume success
        //    var pvPath = _TesterConfig.GetAppPluginsPath() + "Views/ViewResults.cshtml";

        //    //Setup
        //    //var testerService = SetupServices();

        //    //GET DATA TO DISPLAY
        //    var domain = "";
        //    if (ForDomain == "")
        //    {
        //        //Assume current 
        //        domain = Request.RequestUri.Host;
        //    }
        //    else
        //    {
        //        domain = ForDomain;
        //    }
        //    var resultSet = new TestResultSet();
        //    var filesList = new Dictionary<string, DateTime>();
        //    var displayMode = "unknown";
        //    if (ForTimestamp == "")
        //    {
        //        //Show list of available results
        //        displayMode = "list";
        //        var filesMsg = FilesIO.GetListOfFiles(out filesList);
        //        returnStatusMsg.InnerStatuses.Add(filesMsg);
        //    }
        //    else
        //    {
        //        //Get resultset from file
        //        var timestamp = FilesIO.StringToTimestamp(ForTimestamp);
        //        var resultsMsg = FilesIO.ReadResultSet(domain, timestamp, out resultSet);
        //        returnStatusMsg.InnerStatuses.Add(resultsMsg);
        //    }

        //    //VIEW DATA 
        //    var viewData = new ViewDataDictionary();
        //    viewData.Model = returnStatusMsg;
        //    viewData.Add("TestResultSet", resultSet);
        //    viewData.Add("FilesList", filesList);
        //    viewData.Add("DisplayMode", displayMode);

        //    //RENDER
        //    var controllerContext = this.ControllerContext;
        //    var displayHtml =
        //        ApiControllerHtmlHelper.GetPartialViewHtml(controllerContext, pvPath, viewData, HttpContext.Current);
        //    returnSB.AppendLine(displayHtml);

        //    //RETURN AS HTML
        //    return new HttpResponseMessage()
        //    {
        //        Content = new StringContent(
        //            returnSB.ToString(),
        //            Encoding.UTF8,
        //            "text/html"
        //        )
        //    };
        //}

        /// /umbraco/backoffice/Dragonfly/SiteTester/ViewResults
        /// /umbraco/backoffice/Dragonfly/SiteTester/ViewResults?Filename=xxx
        /// /umbraco/backoffice/Dragonfly/SiteTester/ViewResults?Filename=xxx&ResultFilter=xxx
        [HttpGet]
        public IActionResult ViewResults(string Filename = "", string ResultFilter = "")
        {
            //Setup
            var pvPath = RazorFilesPath() + "ViewResults.cshtml";

            //GET DATA TO DISPLAY
            var returnStatusMsg = new StatusMessage(true); //assume success

            //GET DATA TO DISPLAY
            var resultSet = new TestResultSet();

            IEnumerable<FileInfo> filesList = new List<FileInfo>();
            var displayMode = "unknown";
            if (Filename == "")
            {
                //Show list of available results
                displayMode = "list";
                var filesMsg = _filesIo.GetListOfFiles(out filesList);
                returnStatusMsg.InnerStatuses.Add(filesMsg);
            }
            else
            {
                //Get resultset from file
                displayMode = "results";
                var resultsMsg = _filesIo.ReadResultSet(Filename, out resultSet);
                returnStatusMsg.InnerStatuses.Add(resultsMsg);
                returnStatusMsg.Success = resultsMsg.Success;
            }

            var model = returnStatusMsg;

            //VIEW DATA 
            var viewData = new Dictionary<string, object>();
            viewData.Add("TestResultSet", resultSet);
            viewData.Add("FilesList", filesList);
            viewData.Add("DisplayMode", displayMode);
            viewData.Add("ResultFilter", ResultFilter);

            //RENDER
            var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
            var displayHtml = htmlTask.Result;

            //RETURN AS HTML
            var result = new HttpResponseMessage()
            {
                Content = new StringContent(
                    displayHtml,
                    Encoding.UTF8,
                    "text/html"
                )
            };

            return new HttpResponseMessageResult(result);
        }

        /// /umbraco/backoffice/Dragonfly/SiteTester/SearchAllHtml
        [HttpPost]
        public IActionResult SearchAllHtml(SearchFormInputs searchFormInputs)
        {
            //Setup
            var pvPath = RazorFilesPath() + "ViewSearchResults.cshtml";

            //GET DATA TO DISPLAY
            var returnStatusMsg = new StatusMessage(true); //assume success
            var filename = "";
            var trsName = "";
            var phrase = "";

            //GET DATA TO DISPLAY
            var resultSet = new TestResultSet();
            var searchResults = new SearchResultsSet();

            if (searchFormInputs != null)
            {
                filename = searchFormInputs.Filename;
                phrase = searchFormInputs.Phrase;
                var resultsMsg = _filesIo.ReadResultSet(filename, out resultSet);
                returnStatusMsg.InnerStatuses.Add(resultsMsg);

                if (resultsMsg.Success)
                {
                    var configTz = _testerConfig.LocalTimezone;
                    var localTimezone = configTz != "" ? configTz : TimeZoneInfo.Local.Id;
                    trsName = HtmlHelpers.FormatUtcDateTime(_testerConfig, resultSet.TestStartTimestamp.ToUniversalTime(),
                        HtmlHelpers.TimezoneFormatOption.Full, localTimezone);

                    if (resultSet.HasStoredHtml)
                    {
                        //Do search
                        searchResults = _htmlSearchService.GetSearchResultSet(resultSet, phrase);
                    }
                    else
                    {
                        returnStatusMsg.Success = false;
                        returnStatusMsg.Message =
                            $"The Test Result Set for '{trsName}' doesn't include HTML to search.";
                    }
                }
            }
            else
            {
                returnStatusMsg.Success = false;
                returnStatusMsg.Message =
                    $"Search Inputs data was missing.";
            }

            var model = returnStatusMsg;

            //VIEW DATA 
            var viewData = new Dictionary<string, object>();
            viewData.Add("ResultSetFilename", filename);
            viewData.Add("ResultSetDisplayName", trsName);
            viewData.Add("SearchPhrase", phrase);
            viewData.Add("SearchResultsSet", searchResults);

            //RENDER
            var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
            var displayHtml = htmlTask.Result;

            //RETURN AS HTML
            var result = new HttpResponseMessage()
            {
                Content = new StringContent(
                    displayHtml,
                    Encoding.UTF8,
                    "text/html"
                )
            };

            return new HttpResponseMessageResult(result);
        }

        /// /umbraco/backoffice/Dragonfly/SiteTester/GatherOutgoingLinks
        /// /umbraco/backoffice/Dragonfly/SiteTester/GatherOutgoingLinks?Filename=xxx
        [HttpGet]
        public IActionResult GatherOutgoingLinks(string Filename = "")
        {
            //Setup
            var pvPath = RazorFilesPath() + "GatheredLinks.cshtml";

            //GET DATA TO DISPLAY
            var returnStatusMsg = new StatusMessage(true); //assume success

            var resultSet = new TestResultSet();

            IEnumerable<FileInfo> filesList = new List<FileInfo>();
            var displayMode = "unknown";
            var allLinksSet = new LinksSet();
            if (Filename == "")
            {
                //Show list of available results
                displayMode = "list";
                var filesMsg = _filesIo.GetListOfFiles(out filesList);
                returnStatusMsg.InnerStatuses.Add(filesMsg);
            }
            else
            {
                //Get resultset from file
                displayMode = "results";
                var resultsMsg = _filesIo.ReadResultSet(Filename, out resultSet);
                returnStatusMsg.InnerStatuses.Add(resultsMsg);
                returnStatusMsg.Success = resultsMsg.Success;

                //additional data
                if (resultSet.HasLinkTestResults)
                {
                    var resultsWithLinks = resultSet.Results.Where(n => n.OnPageLinks != null);
                    var allTests = resultsWithLinks.SelectMany(n => n.OnPageLinks.LinkTests).ToList();
                    allLinksSet.LinkTests = allTests;
                    allLinksSet.LinkTypesSummary = _siteTesterService.SummarizeLinkTypes(allTests);
                    allLinksSet.DoTestSummary = _siteTesterService.SummarizeLinkDoTests(allTests);
                    allLinksSet.ResultsSummary = _siteTesterService.SummarizeLinkTestResults(allTests);
                }
            }

            var model = returnStatusMsg;

            //VIEW DATA 
            var viewData = new Dictionary<string, object>();
            viewData.Add("TestResultSet", resultSet);
            viewData.Add("FilesList", filesList);
            viewData.Add("DisplayMode", displayMode);
            viewData.Add("FullLinksSet", allLinksSet);

            //RENDER
            var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
            var displayHtml = htmlTask.Result;

            //RETURN AS HTML
            var result = new HttpResponseMessage()
            {
                Content = new StringContent(
                    displayHtml,
                    Encoding.UTF8,
                    "text/html"
                )
            };

            return new HttpResponseMessageResult(result);
        }

        /// /umbraco/backoffice/Dragonfly/SiteTester/ViewException
        [HttpPost]
        public IActionResult ViewException(string ExceptionJson)
        {
            var pvPath = $"{RazorFilesPath()}ExceptionViewer.cshtml";

            //GET DATA TO DISPLAY
            SerializableException exModel = new SerializableException();
            if (ExceptionJson != "")
            { exModel = JsonConvert.DeserializeObject<SerializableException>(ExceptionJson)!; }

            //VIEW DATA 
            var viewData = new Dictionary<string, object>();
            //viewData.Add("DisplayTitle", $"Add additional variables via the View Data as needed....");

            //RENDER
            var htmlTask = Task.Run(() => _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, exModel, viewData));
            var displayHtml = htmlTask.Result;

            if (!string.IsNullOrEmpty(displayHtml))
            {
                //RETURN AS HTML
                var result = new HttpResponseMessage()
                {
                    Content = new StringContent(
                        displayHtml,
                        Encoding.UTF8,
                        "text/html"
                    )
                };
                return new HttpResponseMessageResult(result);
            }
            else
            {
                //string json = JsonConvert.SerializeObject(htmlTask.Exception);

                return new JsonResult(htmlTask);
            }

        }



        #endregion

    }
}