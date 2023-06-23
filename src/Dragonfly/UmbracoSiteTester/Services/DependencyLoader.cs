namespace Dragonfly.UmbracoSiteTester
{
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Umbraco.Cms.Core.Cache;
	using Umbraco.Cms.Core.Services;
	using Umbraco.Cms.Core.Web;
	using Umbraco.Cms.Web.Common;
	using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

	public class DependencyLoader
	{
		public IHostingEnvironment HostingEnvironment { get; }
		public IWebHostEnvironment WebHostingEnvironment { get; }
		public IHttpContextAccessor ContextAccessor { get; }
		public IUmbracoContextAccessor UmbracoContextAccessor { get; }

		public UmbracoHelper UmbHelper;

		public HttpContext Context;

		public ServiceContext Services;

		public AppCaches AppCaches;

		public IConfiguration AppSettingsConfig;

		public Dragonfly.NetHelperServices.FileHelperService DragonflyFileHelperService { get; }

		public DependencyLoader(
			IHostingEnvironment hostingEnvironment,
			IWebHostEnvironment webhostingEnvironment,
			IHttpContextAccessor contextAccessor, IUmbracoContextAccessor umbracoContextAccessor,
			Dragonfly.NetHelperServices.FileHelperService fileHelperService,
			ServiceContext serviceContext,
			AppCaches appCaches,
			IConfiguration appSettingsConfig
		   )
		{
			HostingEnvironment = hostingEnvironment;
			ContextAccessor = contextAccessor;
			UmbracoContextAccessor = umbracoContextAccessor;
			UmbHelper = contextAccessor.HttpContext.RequestServices.GetRequiredService<UmbracoHelper>();
			DragonflyFileHelperService = fileHelperService;
			Context = contextAccessor.HttpContext;
			Services = serviceContext;
			AppCaches = appCaches;
			AppSettingsConfig = appSettingsConfig;
		}
	}
}
