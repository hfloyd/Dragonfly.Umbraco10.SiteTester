namespace Dragonfly.UmbracoSiteTester.Models
{
	public class LinkTest
    {
        public enum LinkType
        {
            External,
            InternalPage,
            InternalPageWithQueryString,
            InternalFile,
            UmbracoMedia,
            UmbracoDoc,
            Unknown,
            Email,
            Asset
        }


        public string Url { get; set; }

        public LinkType Type { get; set; }
        public bool DoTest { get; set; }

        public RenderingResult Result { get; set; }
        public int OnNode { get; set; }
    }
}
