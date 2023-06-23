#pragma warning disable 1591

namespace Dragonfly.UmbracoSiteTester.Composers
{
	using Dragonfly.NetHelperServices;
	using Dragonfly.UmbracoSiteTester.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Umbraco.Cms.Core.Composing;
    using Umbraco.Cms.Core.DependencyInjection;

    public class SetupComposer : IComposer
    {

        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddMvcCore().AddRazorViewEngine();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<DependencyLoader>();
            builder.Services.AddScoped<Dragonfly.UmbracoServices.FileHelperService>();
            builder.Services.AddScoped<Dragonfly.UmbracoSiteTester.SiteTesterService>();
            builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

            //builder.AddUmbracoOptions<Settings>();

        }

    }

}