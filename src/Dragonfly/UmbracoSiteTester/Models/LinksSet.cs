namespace Dragonfly.UmbracoSiteTester.Models
{
	using System.Collections.Generic;

	public class LinksSet
    {
        //public string ResultSetFilename { get; set; }

        //public bool LinksGathered { get; internal set; }
        //public bool LinksTested { get; set; }
        public IEnumerable<LinkTest> LinkTests { get; set; }

        public IEnumerable<string> TypesTested { get; set; }
        public Dictionary<string, long> LinkTypesSummary { get; set; }

        public Dictionary<bool, long> DoTestSummary { get; set; }

        public Dictionary<string, long> ResultsSummary { get; set; }
        public bool TestsPerformed { get; set; }

        public LinksSet()
        {
            this.LinkTests = new List<LinkTest>();
            this.TypesTested = new List<string>();
        }
    }
}
