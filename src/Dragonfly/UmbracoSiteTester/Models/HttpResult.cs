namespace Dragonfly.UmbracoSiteTester.Models
{
	using System;
	using System.Net;

	public class HttpResult
    {
        public HttpStatusCode StatusCode { get; set; }

        public string StatusCodeMessage { get; set; } = "";

        public Exception? ErrorException { get; set; }
    }
}
