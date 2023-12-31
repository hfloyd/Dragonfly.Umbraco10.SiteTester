﻿namespace Dragonfly.UmbracoSiteTester
{
    using Polly;
    using System;
    using Azure.Core;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Static class with various information and constants about the package.
    /// </summary>
    public class SiteTesterPackage
    {
        /// <summary>
        /// Gets the alias of the package.
        /// </summary>
        public const string Alias = "Dragonfly.Umbraco10.SiteTester";

        /// <summary>
        /// Gets the friendly name of the package.
        /// </summary>
        public const string Name = "Dragonfly Site Tester (for Umbraco 10)";

        /// <summary>
        /// Gets the version of the package.
        /// </summary>
        public static readonly Version Version = typeof(SiteTesterPackage).Assembly.GetName().Version;

        /// <summary>
        /// Gets the URL of the GitHub repository for this package.
        /// </summary>
        public const string GitHubUrl = "https://github.com/hfloyd/Dragonfly.Umbraco10.SiteTester";

        /// <summary>
        /// Gets the URL of the issue tracker for this package.
        /// </summary>
        public const string IssuesUrl = "https://github.com/hfloyd/Dragonfly.Umbraco10.SiteTester/issues";

        /// <summary>
        /// Gets the URL of the documentation for this package.
        /// </summary>
        public const string DocumentationUrl = "https://github.com/hfloyd/Dragonfly.Umbraco10.SiteTester#documentation";



    }

    //public static class TempExtensions
    //{
       
    //}
}
