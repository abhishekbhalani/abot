using Abot.Poco;
using System;
using System.Collections.Generic;

namespace Abot.Core
{
    public unsafe class LightHyperLinkParser : IHyperLinkParser
    {
        public readonly static long href;
        public readonly static long HREF;

        static LightHyperLinkParser()
        {
            fixed (char* phref = "href", pHREF = "HREF")
            {
                href = *(long*)phref;
                HREF = *(long*)pHREF;
            }
        }

        public IEnumerable<Uri> GetLinks(Uri pageUri, string pageHtml)
        {
            List<Uri> uris = new List<Uri>();

            fixed (char* html = pageHtml)
            {
                char* start = html;
                char* end = start + pageHtml.Length - sizeof(long);

                char* current = start;

                bool insideTag = false;
                bool insideLink = false;
                while (current != end)
                {
                    if (*current == '<')
                    {
                        insideTag = true;
                    }
                    else if (*current == '>')
                    {
                        insideTag = false;
                    }
                    else if (insideTag)
                    {
                        //If we're in between square brackets, start looking for HREF or href
                        if (insideLink)
                        {
                            //If we found a link, extract it
                            string uri = ScanLink(current, end);
                            if (!string.IsNullOrEmpty(uri))
                            {
                                try
                                {
                                    uris.Add(new Uri(pageUri, uri));
                                }
                                catch (UriFormatException ex)
                                {
                                    log4net.LogManager.GetLogger(typeof(LightHyperLinkParser)).WarnFormat("Failed to parse uri {0} - {1}", uri, ex.Message);
                                }
                            }
                            
                            //fast forward past the link we just extracted
                            current += uri.Length;

                            insideLink = false;
                        }
                        else
                        {
                            //See if the text at the current position is either HREF or href
                            long* currentChunk = (long*)current;
                            if (*currentChunk == href || *currentChunk == HREF)
                            {
                                //Fast forward past 'href' and indicate that we need to extract a link
                                current += sizeof(long) / sizeof(char);
                                insideLink = true;
                            }
                        }
                    }

                    current++;
                }
            }

            return uris;
        }

        public IEnumerable<Uri> GetLinks(CrawledPage crawledPage)
        {
            return GetLinks(crawledPage.Uri, crawledPage.RawContent);
        }

        private static string ScanLink(char* start, char* end)
        {
            char* current = start;
            char* linkStart = default(char*);
            char* linkEnd = default(char*);
            while (current != end)
            {
                //Assume a link will begin and terminate when one of these characters is encountered
                bool isSpecialCharacter = *current == '=' || *current == '"' || *current == '\'' || *current == ' ' || *current == '>';

                //If we don't have a link start yet...
                if (linkStart == default(char*))
                {
                    //and we're not looking at a special character...
                    if (!isSpecialCharacter)
                    {
                        linkStart = current;
                    }
                }
                else
                {
                    //If we already determined the start of the link and we're looking at a special character...
                    if (isSpecialCharacter)
                    {
                        linkEnd = current;
                        break;
                    }
                }

                current++;
            }

            string link = string.Empty;

            if (linkEnd > linkStart)
            {
                link = new string(linkStart, 0, (int)(linkEnd - linkStart));
            }

            return link;
        }
    }
}
