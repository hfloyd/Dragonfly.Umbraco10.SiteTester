namespace Dragonfly.UmbracoSiteTester.Models
{
	using System;
	using System.Collections.Generic;
	using Dragonfly.NetModels;

	public class SearchResultsSet
    {
        public string SiteDomain { get; set; }
        public DateTime SearchStartTimestamp { get; set; }
        public DateTime SearchEndTimestamp { get; set; }
       
        public IEnumerable<RenderingResult> Results { get; set; }

        public long QtyNodesSearched { get; set; }
        public long QtyResultsFound { get; set; }
        public StatusMessage Message { get; set; }
    }
}
