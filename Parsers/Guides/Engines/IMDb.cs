﻿namespace RoliSoft.TVShowTracker.Parsers.Guides.Engines
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using HtmlAgilityPack;

    using NUnit.Framework;

    /// <summary>
    /// Provides support for scraping IMDb pages.
    /// </summary>
    [TestFixture]
    public class IMDb : Guide
    {
        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return "IMDb";
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
                return "http://www.imdb.com/";
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
                return "/RSTVShowTracker;component/Images/imdb.png";
            }
        }

        /// <summary>
        /// Gets the ID of a TV show in the database.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>ID.</returns>
        public override IEnumerable<ShowID> GetID(string name, string language = "en")
        {
            var html  = Utils.GetHTML("http://www.imdb.com/search/title?title_type=tv_series&title=" + Uri.EscapeUriString(name), headers: new Dictionary<string, string> { { "Accept-Language", "en-US" } });
            var shows = html.DocumentNode.SelectNodes("//td[@class='title']");

            if (shows == null)
            {
                yield break;
            }

            foreach (var show in shows)
            {
                var id  = new ShowID();
                
                id.URL      = Site.TrimEnd('/') + show.GetNodeAttributeValue("a", "href");
                id.ID       = Regex.Match(id.URL, @"/tt(\d+)/").Groups[1].Value;
                id.Title    = HtmlEntity.DeEntitize(show.GetNodeAttributeValue("../td[@class='image']//img", "title")).Replace(" TV Series", string.Empty);
                id.Cover    = show.GetNodeAttributeValue("../td[@class='image']//img", "src");
                id.Language = "en";

                yield return id;
            }
        }

        /// <summary>
        /// Extracts the data available in the database.
        /// </summary>
        /// <param name="id">The ID of the show.</param>
        /// <returns>TV show data.</returns>
        public override TVShow GetData(string id, string language = "en")
        {
            var summary = Utils.GetHTML("http://www.imdb.com/title/tt{0}/".FormatWith(id), headers: new Dictionary<string, string> { { "Accept-Language", "en-US" } });
            var show    = new TVShow();

            show.Title       = HtmlEntity.DeEntitize(summary.DocumentNode.GetNodeAttributeValue("//td[@id='img_primary']//img", "title")).Replace(" Poster", string.Empty);
            show.Description = HtmlEntity.DeEntitize((summary.DocumentNode.GetTextValue("//h2[text()='Storyline']/following-sibling::p[1]") ?? string.Empty));
            show.Cover       = summary.DocumentNode.GetNodeAttributeValue("//td[@id='img_primary']//img", "src");
            show.Airing      = !Regex.IsMatch(summary.DocumentNode.GetTextValue("//span[@class='tv-series-smaller']") ?? string.Empty, @"\d{4}–\d{4}");
            show.URL         = "http://www.imdb.com/title/tt{0}/".FormatWith(id);
            show.Episodes    = new List<TVShow.Episode>();

            var runtime  = summary.DocumentNode.GetTextValue("//h4[text()='Runtime:']/following-sibling::text()");
            show.Runtime = runtime != null
                           ? Regex.Match(runtime, @"\d{2,3}").Value.ToInteger()
                           : 30;

            var genres = summary.DocumentNode.SelectNodes("//h4[text()='Genres:']/following-sibling::a");
            if (genres != null)
            {
                foreach (var genre in genres)
                {
                    show.Genre += genre.InnerText + ", ";
                }

                show.Genre = show.Genre.TrimEnd(", ".ToCharArray());
            }
            
            var listing  = Utils.GetHTML("http://www.imdb.com/title/tt{0}/episodes".FormatWith(id));
            var nodes    = listing.DocumentNode.SelectNodes("//td/h3");

            if (nodes == null)
            {
                return show;
            }

            foreach (var node in nodes)
            {
                var number = Regex.Match(HtmlEntity.DeEntitize(node.InnerText), @"Season (?<season>\d+), Episode (?<episode>\d+): (?<title>.+)");

                if (!number.Success) { continue; }

                var ep = new TVShow.Episode();

                ep.Season  = number.Groups["season"].Value.ToInteger();
                ep.Number  = number.Groups["episode"].Value.ToInteger();
                ep.Title   = number.Groups["title"].Value;
                ep.Summary = HtmlEntity.DeEntitize((node.GetTextValue("../br/following-sibling::text()") ?? string.Empty).Trim());

                var url = node.GetNodeAttributeValue("a", "href");
                if (url != null)
                {
                    ep.URL = Site.TrimEnd('/') + url;
                }

                DateTime dt;
                ep.Airdate = DateTime.TryParseExact(node.GetTextValue("../span/strong") ?? string.Empty, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                             ? dt
                             : Utils.UnixEpoch;

                show.Episodes.Add(ep);
            }

            return show;
        }
    }
}