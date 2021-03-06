﻿namespace RoliSoft.TVShowTracker.Parsers.WebSearch.Engines
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using NUnit.Framework;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides support for searching on Google.
    /// </summary>
    [TestFixture]
    public class Google : WebSearchEngine
    {
        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return "Google";
            }
        }

        /// <summary>
        /// Gets the URL of the site.
        /// </summary>
        /// <value>The site location.</value>
        public override string Site
        {
            get
            {
                return "http://www.google.com/";
            }
        }

        /// <summary>
        /// Gets the name of the plugin's developer.
        /// </summary>
        /// <value>The name of the plugin's developer.</value>
        public override string Developer
        {
            get
            {
                return "RoliSoft";
            }
        }

        /// <summary>
        /// Gets the version number of the plugin.
        /// </summary>
        /// <value>The version number of the plugin.</value>
        public override Version Version
        {
            get
            {
                return Utils.DateTimeToVersion("2012-12-30 11:35 AM");
            }
        }

        /// <summary>
        /// Searches for the specified query with this service.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>First link on the search result or empty string.</returns>
        public override IEnumerable<SearchResult> Search(string query)
        {
            var search = Utils.GetURL(Site + "uds/GwebSearch?callback=google.search.WebSearch.RawCompletion&context=0&lstkp=0&rsz=small&hl=en&source=gsc&gss=.com&sig=22c4e39868158a22aac047a2c138a780&q={0}&gl=www.google.com&qid=12a9cb9d0a6870d28&key=AIzaSyA5m1Nc8ws2BbmPRwKu5gFradvD_hgq6G0&v=1.0".FormatWith(Utils.EncodeURL(query)));
            var json   = JObject.Parse(Regex.Replace(search, @"}[^}]+$", "}").Remove(0, "google.search.WebSearch.RawCompletion('0',".Length));

            if (!json["results"].HasValues)
            {
                yield break;
            }

            foreach (var result in json["results"])
            {
                yield return new SearchResult
                    {
                        Title = Regex.Replace(result["title"].Value<string>(), @"<.*?>", string.Empty),
                        URL   = result["unescapedUrl"].Value<string>()
                    };
            }
        }
    }
}