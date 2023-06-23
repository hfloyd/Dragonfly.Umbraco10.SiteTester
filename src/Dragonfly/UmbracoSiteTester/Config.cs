namespace Dragonfly.UmbracoSiteTester
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Config
    {
	    //As per https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#bind-hierarchical-configuration-data-using-the-options-pattern-1

	    //AppSettings Config Section name
	    public const string ConfigSectionName = "DragonflySiteTester";
	    
         #region Properties

        [JsonProperty("HttpUrl")]
        public string HttpUrl { get; set; }= "http://127.0.0.1/default.aspx";

        [JsonProperty("HttpHost")] 
        public string HttpHost { get; set; } = "";

        [JsonProperty("HttpTimeout")]
        public int HttpTimeout { get; set; } = 360;

        [JsonProperty("ScriptTimeout")] 
        public int ScriptTimeout { get; set; } = 1200;

        [JsonProperty("DataStoragePath")]
        public string DataStoragePath { get; set; } = "/App_Data/DragonflySiteTester/";

        [JsonProperty("StoreRenderedHtml")] 
        public bool StoreRenderedHtml { get; set; } = false;

        [JsonProperty("AlsoTestOnPageLinks")]
        public bool AlsoTestOnPageLinks { get; set; }= false;

        [JsonProperty("LinkTypesToTest")]
        public List<string> LinkTypesToTest { get; set; }

        [JsonProperty("InternalDomains")]
        public List<string> InternalDomains { get; set; }

        [JsonProperty("PageExtensions")]
        public List<string> PageExtensions { get; set; }

        [JsonProperty("LocalTimezone")]
        public string LocalTimezone { get; set; }= "";

        [JsonProperty("InlineErrorStrings")]
        public List<string> InlineErrorStrings { get; set; }

        #endregion

        public string GetAppPluginsPath()//bool ReturnMapped = false)
        {
            var path = "~/App_Plugins/Dragonfly.SiteTester/";

            //if (ReturnMapped)
            //{
            //    return HostingEnvironment.MapPath(path);
            //}
            //else
            //{
               return path;
            //}
        }


        //public static Config GetConfig()
        //{
        //    string path = "~/Config/DragonflySiteTester.config";
        //    string pathMapped = HostingEnvironment.MapPath(path);
        //    var lastModified = File.GetLastWriteTime(pathMapped);

        //    if (_config == null || lastModified > _config.ConfigTimestamp)
        //    {
        //        XmlSerializer serializer = new XmlSerializer(typeof(Config));

        //        // Read from file
        //        try
        //        {
        //            using (var reader = new StreamReader(pathMapped))
        //            {
        //                _config = (Config)serializer.Deserialize(reader);
        //            }
        //        }
        //        catch (InvalidOperationException ex)
        //        {
        //            throw new InvalidOperationException("The format of 'DragonflySiteTester.config' is invalid. Details: " + ex.Message, ex.InnerException);
        //        }

        //        _config.ConfigTimestamp = lastModified;
        //    }

        //    return _config;

        //}



        //public string GetDataPath(bool ReturnMapped = false)
        //{
        //    var pathDefault = $"/App_Data/Umbraco8SiteTester/";

        //    var path = _config.DataStoragePath != "" ? _config.DataStoragePath : pathDefault;

        //    if (ReturnMapped)
        //    {
        //        return HostingEnvironment.MapPath(path);
        //    }
        //    else
        //    {
        //        return path;
        //    }
        //}

        //public string GetHttpUrl()
        //{
        //    var defaultVal = "http://127.0.0.1/default.aspx";
        //    var webConfigValue = this.;

        //    var finalValue = "";

        //    if (webConfigValue != null)
        //    {
        //        finalValue = webConfigValue;
        //    }
        //    else if (_config.HttpUrl != "")
        //    {
        //        finalValue = _config.HttpUrl;
        //    }
        //    else
        //    {
        //        finalValue = defaultVal;
        //    }

        //    return finalValue;
        //}

        //public string GetHttpHost()
        //{
        //    var defaultVal = "";
        //    var webConfigValue = ConfigurationManager.AppSettings["DragonflySiteTesterHttpHost"];

        //    var finalValue = "";

        //    if (webConfigValue != null)
        //    {
        //        finalValue = webConfigValue;
        //    }
        //    else if (_config.HttpHost != "")
        //    {
        //        finalValue = _config.HttpHost;
        //    }
        //    else
        //    {
        //        finalValue = defaultVal;
        //    }

        //    return finalValue;
        //}



    }
}
