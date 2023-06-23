namespace Dragonfly.UmbracoSiteTester
{
	using System;
	using System.CodeDom;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Web;
	using Dragonfly.UmbracoSiteTester.Models;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Logging;
	using Umbraco.Cms.Core.Cache;
	using Umbraco.Cms.Core.Models;
	using Umbraco.Cms.Core.Services;
	using Umbraco.Cms.Core.Web;
	using Umbraco.Cms.Web.Common;
	using Umbraco.Cms.Web.Common.UmbracoContext;

	public class HttpRenderingService
	{
		private readonly ILogger _logger;
		private readonly AppCaches _appCaches;
		private readonly IUmbracoContext _umbracoContext;
		private readonly HttpContext _httpContext;
		private readonly UmbracoHelper _umbracoHelper;
		private readonly ServiceContext _services;

		public int RequestTimeoutSeconds { get; set; }

		public int ScriptTimeoutSeconds { get; set; }

		/// <summary>
		/// Needed for HTTP Rendering, determines URL of the default.aspx page on your host.I recommend you use
		/// 127.0.0.1 and set the host header below to your domain to avoid
		/// problems with name resolution, firewalls, etc.
		/// </summary>
		public string HttpUrl { get; set; }

		/// <summary>
		/// Domain name of your site e.g.www.yoursite.com
		/// </summary>
		public string HttpHost { get; set; }

		public HttpRenderingService(DependencyLoader dependencies,
			ILogger logger,
			string HttpHost,
			string HttpUrl = "http://127.0.0.1/default.aspx",
			int RequestTimeoutSeconds = 120, int ScriptTimeoutSeconds = 1200)
		{
			this._logger = logger;
			this._httpContext = dependencies.Context;
			this._appCaches = dependencies.AppCaches;
			var hasUmbContext = dependencies.UmbracoContextAccessor.TryGetUmbracoContext(out _umbracoContext);
			this._umbracoHelper = dependencies.UmbHelper;
			this._services = dependencies.Services;

			this.HttpUrl = HttpUrl;
			this.HttpHost = HttpHost;
			this.RequestTimeoutSeconds = RequestTimeoutSeconds;
			this.ScriptTimeoutSeconds = ScriptTimeoutSeconds;
		}

		internal HttpResult HttpRenderNode(IContent Content, out string Html)
		{
			// this can take a while, if we're running sync this is needed
			SetScriptTimeout();

			if (Content == null)
			{
				_logger.LogDebug("{Function} : {Message} : {NodeId}", "Dragonfly.UmbracoSiteTester.HttpRenderingService.HttpRenderNode", "Content Is NULL", "NULL");

				Html = "";
				return null;
			}

			var success = RenderNodeViaHttp(Content.Id, out string fullHtml);

			Html = fullHtml;
			return success;
		}

		internal HttpResult HttpRenderUrl(string Url, bool IsExternal, out string Html)
		{
			var url = Url;

			// this can take a while, if we're running sync this is needed
			SetScriptTimeout();

			var defaultBaseUrl = this.HttpHost;
			if (!defaultBaseUrl.StartsWith("http"))
			{
				defaultBaseUrl = $"http://{defaultBaseUrl}";
			}
			if (string.IsNullOrEmpty(defaultBaseUrl))
			{ throw new ArgumentException("HttpHost must be set to use Http Url rendering"); }

			//Make sure we have a full Url
			if (url.StartsWith("~"))
			{
				url = url.Replace("~", defaultBaseUrl);
			}
			else if (Url.StartsWith("/") && !Url.StartsWith("//"))
			{
				url = $"{defaultBaseUrl}{url}";
			}

			var success = RenderUrlViaHttp(url, IsExternal, out string fullHtml);

			Html = fullHtml;
			return success;
		}


		/// <summary>
		/// Given a string (from config) denoting number of minutes, set HTTP timout
		/// to the proper number of sectonds
		/// </summary>
		private void SetScriptTimeout()
		{
			//if (_httpContext != null)
			//{
			//    HttpContext.Current.Server.ScriptTimeout = this.ScriptTimeoutSeconds;
			//}
		}

		//public int SetTimeoutOption(string SecondsString)
		//{
		//    int secondsInt;
		//    if (!string.IsNullOrEmpty(SecondsString) && Int32.TryParse(SecondsString, out secondsInt))
		//    {
		//        return secondsInt;
		//    }
		//    else
		//    {
		//        return 120;
		//    }
		//}


		/// <summary>
		/// Use Http Web Requests to render a node to a string
		/// </summary>
		/// <remarks>
		/// this calls Umbraco's default.aspx rather than attempt to figure out
		/// the standard umbraco "nice" url. Simply because we can't get the
		/// nice URL without a valid Http Context in the first place. Also, 
		/// the query string we pass to the client page in RenderTemplate
		/// is replaced with a cookie here, simply because adding items
		/// to the query string for default.aspx doesn't actually make
		/// them visible to the page being rendered. Grrrrrrr. 
		/// </remarks>
		/// <param name="PageId"></param>
		/// <param name="cookieDictionary"></param>
		/// <param name="FullHtml"></param>
		/// <returns></returns>
		private HttpResult RenderNodeViaHttp(int PageId, out string FullHtml)
		{
			var defaultUrl = this.HttpUrl;
			if (string.IsNullOrEmpty(defaultUrl))
			{ throw new ArgumentException("HttpUrl must be set to use Http node rendering"); }

			var firstSeparator = "?";
			if (defaultUrl.Contains('?'))
				firstSeparator = "&";

			var url = string.Format("{0}{1}umbpageid={2}", defaultUrl, firstSeparator, PageId);

			//get timeout
			var httpTimeout = this.RequestTimeoutSeconds * 1000;

			//get host header
			var host = this.HttpHost;

			// setup request
			var webRequest = (HttpWebRequest)WebRequest.Create(url);

			try
			{
				if (!string.IsNullOrEmpty(host))
				{ webRequest.Host = host; }
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error in RenderNodeViaHttp() - Cannot set Host to '{HostName}'", host);
				throw;
			}

			webRequest.Timeout = httpTimeout;
			webRequest.ReadWriteTimeout = httpTimeout;
			webRequest.ServicePoint.Expect100Continue = false;
			webRequest.UserAgent = "SiteTesterService";
			webRequest.KeepAlive = true;
			webRequest.Method = "GET";
			webRequest.ContentType = "text/html";
			webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			try
			{
				var result = TryRequest(webRequest, out FullHtml);
				if (string.IsNullOrEmpty(FullHtml))
				{
					_logger.LogDebug("{Function} : {Message} : {Url}", "Dragonfly.UmbracoSiteTester.HttpRenderingService.RenderNodeViaHttp", "No HTML returned", webRequest.RequestUri);
				}

				if (result.ErrorException != null)
				{
					_logger.LogDebug("{Function} : {Message} : {Url}", "Dragonfly.UmbracoSiteTester.HttpRenderingService.RenderNodeViaHttp", result.ErrorException.Message, webRequest.RequestUri);
				}

				return result;
			}
			catch (WebException ex)
			{
				if (ex.Message.Contains("The underlying connection was closed"))
				{
					try
					{
						//Try once more
						webRequest.KeepAlive = false;
						var result = TryRequest(webRequest, out FullHtml);
						return result;
					}
					catch (Exception e)
					{
						_logger.LogError(e, "Error in RenderNodeViaHttp() Timeout:{TimeoutSet} HttpUrl: {HttpUrl}", httpTimeout, defaultUrl);
						throw;
					}

				}
				else
				{
					_logger.LogError(ex, "Error in RenderNodeViaHttp() Timeout:{TimeoutSet} HttpUrl: {HttpUrl}", httpTimeout, defaultUrl);
					throw;
				}
			}
			finally
			{
				webRequest.Abort();
			}
		}

		/// <summary>
		/// Use Http Web Requests to render a Url to a string
		/// </summary>
		/// <param name="Url">Url to Render</param>
		/// <param name="IsExternal"></param>
		/// <param name="FullHtml"></param>
		/// <returns></returns>
		private HttpResult RenderUrlViaHttp(string Url, bool IsExternal, out string FullHtml)
		{
			//get timeout
			var httpTimeout = this.RequestTimeoutSeconds * 1000;

			//get host header
			var host = this.HttpHost;

			// setup request
			var webRequest = (HttpWebRequest)WebRequest.Create(Url);

			try
			{
				if (!string.IsNullOrEmpty(host))
				{ webRequest.Host = host; }
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error in RenderNodeViaHttp() - Cannot set Host to '{HostName}'", host);
				throw;
			}

			webRequest.Timeout = httpTimeout;
			webRequest.ReadWriteTimeout = httpTimeout;
			webRequest.ServicePoint.Expect100Continue = false;
			webRequest.UserAgent = "SiteTesterService";
			webRequest.KeepAlive = true;
			webRequest.Method = "GET";
			webRequest.ContentType = "text/html";
			webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			if (IsExternal)//(webRequest.RequestUri.Scheme == "https")
			{
				webRequest.Method = "HEAD";
				System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
				//ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
			}

			try
			{
				var result = TryRequest(webRequest, out FullHtml);
				return result;
			}
			catch (WebException ex)
			{
				if (ex.Message.Contains("The underlying connection was closed"))
				{
					try
					{
						//Try once more
						// webRequest.KeepAlive = false;
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
						var result = TryRequest(webRequest, out FullHtml);
						return result;
					}
					catch (Exception e)
					{
						_logger.LogError(ex, "Error in RenderNodeViaHttp() Timeout:{TimeoutSet} HttpUrl: {HttpUrl}", httpTimeout, HttpUrl);
						throw;
					}

				}
				else
				{
					_logger.LogError(ex, "Error in RenderNodeViaHttp() Timeout:{TimeoutSet} HttpUrl: {HttpUrl}", httpTimeout, HttpUrl);
					throw;
				}
			}
			finally
			{
				webRequest.Abort();
			}
		}



		private static HttpResult TryRequest(HttpWebRequest WebRequest, out string FullHtml)
		{
			var resultStatus = new HttpResult();

			try
			{
				using (HttpWebResponse webResponse = (HttpWebResponse)WebRequest.GetResponse())
				{
					using (Stream objStream = webResponse.GetResponseStream())
					{
						using (StreamReader objReader = new StreamReader(objStream))
						{
							FullHtml = objReader.ReadToEnd();
							objReader.Close();
						}

						objStream.Flush();
						objStream.Close();
					}

					resultStatus.StatusCode = webResponse.StatusCode;
					resultStatus.StatusCodeMessage = webResponse.StatusDescription;
					webResponse.Close();
				}
			}
			catch (WebException ex)
			{
				HttpWebResponse webResponse = (HttpWebResponse)ex.Response;
				if (webResponse != null)
				{
					if (webResponse.StatusCode == HttpStatusCode.NotFound)
					{
						resultStatus.StatusCode = HttpStatusCode.NotFound;
						resultStatus.StatusCodeMessage = ex.Message;
					}
					else
					{
						resultStatus.StatusCode = webResponse.StatusCode;
						resultStatus.StatusCodeMessage = ex.Message;
					}
				}

				resultStatus.ErrorException = ex;
				FullHtml = "HTTP RENDERING WebException: " + ex.Message;
			}
			catch (Exception e)
			{
				var hasStatusCode = resultStatus.StatusCode != 0;
				if (!hasStatusCode)
				{
					if (e.Message.Contains("404"))
					{
						resultStatus.StatusCode = HttpStatusCode.NotFound;
						resultStatus.StatusCodeMessage = e.Message;
					}
					else
					{
						resultStatus.StatusCode = HttpStatusCode.InternalServerError;
						resultStatus.StatusCodeMessage = e.Message;
					}
				}

				resultStatus.ErrorException = e;
				FullHtml = "HTTP RENDERING Exception: " + e.Message;
			}

			return resultStatus;
		}

	}
}

