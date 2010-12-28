﻿namespace RoliSoft.TVShowTracker.Parsers.Downloads.Engines.Torrent
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides support for scraping freshon.tv.
    /// </summary>
    [Parser("RoliSoft", "2010-12-09 2:21 AM")]
    public class TvTorrentsRo : DownloadSearchEngine
    {
        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return "Tv Torrents Ro";
            }
        }

        /// <summary>
        /// Gets the URL to the favicon of the site.
        /// </summary>
        /// <value>The icon location.</value>
        public override string Icon
        {
            get
            {
                return "http://static.freshon.tv/favicon.ico";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the site requires cookies to authenticate.
        /// </summary>
        /// <value><c>true</c> if requires cookies; otherwise, <c>false</c>.</value>
        public override bool RequiresCookies
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the type of the link.
        /// </summary>
        /// <value>The type of the link.</value>
        public override Types Type
        {
            get
            {
                return Types.Torrent;
            }
        }

        /// <summary>
        /// Searches for download links on the service.
        /// </summary>
        /// <param name="query">The name of the release to search for.</param>
        /// <returns>List of found download links.</returns>
        public override IEnumerable<Link> Search(string query)
        {
            var html  = Utils.GetHTML("http://freshon.tv/browse.php?search=" + Uri.EscapeUriString(query), cookies: Cookies);
            var links = html.DocumentNode.SelectNodes("//table/tr/td/div[1]/a");

            if (links == null)
            {
                yield break;
            }

            foreach (var node in links)
            {
                yield return new Link
                    {
                        Site    = Name,
                        Release = node.GetAttributeValue("title", string.Empty),
                        URL     = "http://freshon.tv/download.php?id=" + Regex.Replace(node.GetAttributeValue("href", string.Empty), "[^0-9]+", string.Empty) + "&type=torrent",
                        Size    = node.SelectSingleNode("../../../td[@class='table_size']").InnerHtml.Trim().Replace("<br>", " "),
                        Quality = ThePirateBay.ParseQuality(node.GetAttributeValue("title", string.Empty)),
                        Type    = Types.Torrent
                    };
            }
        }
    }
}