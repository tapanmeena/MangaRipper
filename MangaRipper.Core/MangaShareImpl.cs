﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRipper.Core
{
    class MangaShareImpl : IManga
    {
        public async Task<IList<Chapter>> FindChapters(string manga)
        {
            var downloader = new Downloader();
            var parser = new Parser();

            // find all chapters in a manga
            string input = await downloader.DownloadStringAsync(manga);
            string regEx = "<td class=\"datarow-0\"><a href=\"(?<Value>[^\"]+)\"><img src=\"http://read.mangashare.com/static/images/dlmanga.gif\" class=\"inlineimg\" border=\"0\" alt=\"(?<Name>[^\"]+)\" /></a></td>";
            var chaps = parser.ParseGroup(regEx, input, "Name", "Value");

            return chaps;
        }

        public async Task<IList<string>> FindImanges(Chapter chapter)
        {
            var downloader = new Downloader();
            var parser = new Parser();

            // find all pages in a chapter
            string input = await downloader.DownloadStringAsync(chapter.Link);
            string regExPages = @"<select name=""pagejump"" class=""page"" onchange=""javascript:window.location='(?<Value>[^']+)'\+this\.value\+'\.html';"">";
            var pageBase = parser.Parse(regExPages, input, "Value").FirstOrDefault();

            var pagesExtend = parser.Parse(@"<option value=""(?<FileName>\d+)"">Page \d+</option>", input, "FileName");

            // transform pages link
            pagesExtend = pagesExtend.Select(p => {
                string baseLink = pageBase + p + ".html";
                var value = new Uri(new Uri(chapter.Link), baseLink).AbsoluteUri;
                return value;
            }).ToList();

            // find all images in pages
            var pageData = await downloader.DownloadStringAsync(pagesExtend);
            var images = parser.Parse(@"<img src=""(?<Value>[^""]+)"" border=""0"" alt=""[^""]+"" />\n", pageData, "Value");

            return images;
        }

        public SiteInformation GetInformation()
        {
            return new SiteInformation("MangaShare", "http://read.mangashare.com/", "English");
        }

        public bool Of(string link)
        {
            var uri = new Uri(link);
            return uri.Host.Equals("read.mangashare.com");
        }
    }
}
