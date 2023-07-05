#pragma warning disable 1591

namespace Dragonfly.UmbracoSiteTester
{
	using Dragonfly.NetHelperServices;
    using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Umbraco.Cms.Core.Composing;
    using Umbraco.Cms.Core.DependencyInjection;
	using Umbraco.Cms.Core.Services;

    public class SetupComposer : IComposer
    {

        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddMvcCore().AddRazorViewEngine();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddHttpContextAccessor();
            
            //To allow for usage of ILogger in static functions
                 
            builder.Services.AddScoped<DependencyLoader>();
            builder.Services.AddScoped<Dragonfly.NetHelperServices.FileHelperService>();
            builder.Services.AddScoped<Dragonfly.UmbracoSiteTester.HttpRenderingService>();
            builder.Services.AddScoped<Dragonfly.UmbracoSiteTester.FilesIO>();
            builder.Services.AddScoped<Dragonfly.UmbracoSiteTester.HtmlSearchService>();
            builder.Services.AddScoped<Dragonfly.UmbracoSiteTester.SiteTesterService>();
            
            builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

            //builder.AddUmbracoOptions<Settings>();

        }

    }

}