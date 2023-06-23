namespace Dragonfly.UmbracoSiteTester.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Dragonfly.NetModels;
    using Dragonfly.UmbracoSiteTester.Models;
    using Microsoft.Extensions.Logging;
    using Umbraco.Cms.Core.Cache;
    using Umbraco.Cms.Core.Services;
    using Umbraco.Cms.Core.Web;
    using Umbraco.Cms.Web.Common;
    using Umbraco.Cms.Web.Common.UmbracoContext;

    internal class HtmlSearchService
    {
        private readonly ILogger _logger;
        private readonly AppCaches _appCaches;
        private readonly IUmbracoContext _umbracoContext;
        private readonly UmbracoHelper _umbracoHelper;
        private readonly ServiceContext _services;

        private List<RenderingResult> _ResultList = new List<RenderingResult>();

        private TestResultSet _TestResultSet = new TestResultSet();

        public HtmlSearchService(DependencyLoader dependencies,
	        ILogger logger,
	        TestResultSet ResultSetToSearch)
        {
            this._logger = logger;
            this._appCaches = dependencies.AppCaches;
            var hasUmbContext = dependencies.UmbracoContextAccessor.TryGetUmbracoContext(out _umbracoContext);
            this._umbracoHelper = dependencies.UmbHelper;
            this._services = dependencies.Services;
            _TestResultSet = ResultSetToSearch;
        }

        public SearchResultsSet GetSearchResultSet(string SearchPhrase)
        {
            var srs = new SearchResultsSet();
            var msg = new StatusMessage();

            srs.SiteDomain = _TestResultSet.SiteDomain;
            srs.SearchStartTimestamp = DateTime.Now.ToUniversalTime();

            //TODO: This is probably very slow... not sure what would be an easy substitute...
            if (_TestResultSet == null)
            {
                msg.Success = false;
                msg.Message = "Original Test Result Set was NULL";
            }
            else if (_TestResultSet.Results == null)
            {
                msg.Success = false;
                msg.Message = "Original Test Result Set has NULL Results List";
            }
            else
            {
                var nonNullResults = _TestResultSet.Results.Where(n => n.RenderedOutput != null).ToList();
                srs.QtyNodesSearched = nonNullResults.Count;
                var searchResults = nonNullResults.Where(n => n.RenderedOutput.Contains(SearchPhrase)).ToList();
                srs.QtyResultsFound = searchResults.Count;
                srs.Results = searchResults;
                msg.Success = true;
                msg.Message = $"Search for '{SearchPhrase}' completed. {searchResults.Count} results found.";
            }

            srs.SearchEndTimestamp = DateTime.Now.ToUniversalTime();
            srs.Message = msg;
            return srs;
        }
    }
}
