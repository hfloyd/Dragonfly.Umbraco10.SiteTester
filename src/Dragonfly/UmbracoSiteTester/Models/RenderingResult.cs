namespace Dragonfly.UmbracoSiteTester.Models
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
	using Umbraco.Cms.Core;

	public enum ResultType
    {
        Ok,
        CheckPossibleInlineError,
        Redirected,
        ErrorRequestException,
        ErrorHttpOther,
        Error404,
        Error500,
        NotTestedProtected,
        NotTestedUnPublished,
        Unknown
    }
    public class RenderingResult
    {

        public int NodeId { get; set; }

        public Udi NodeUdi { get; set; }

        public string NodeName { get; set; }

        public string Url { get; set; }

        public string ContentTypeAlias { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultType Result { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public HttpStatusCode StatusCode { get; set; }

        public string StatusCodeMessage { get; set; }
        
        public Exception ErrorException { get; set; }

        public IEnumerable<string> ContentErrorMatches { get; set; }

        public string RenderedOutput { get; set; }

        public LinksSet OnPageLinks { get; set; }

    }
}
