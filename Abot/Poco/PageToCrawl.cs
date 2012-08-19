using System;

namespace Abot.Poco
{
    public class PageToCrawl
    {
        public PageToCrawl(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            Uri = uri;
        }

        /// <summary>
        /// The uri of the page
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The parent uri of the page
        /// </summary>
        public Uri ParentUri { get; set; }

        /// <summary>
        /// Whether http requests had to be retried more than once. This could be due to throttling or politeness.
        /// </summary>
        public bool IsRetry { get; set; }

        ///// <summary>
        ///// Returns the root Uri of this page
        ///// </summary>
        //public Uri GetDomainUri()
        //{
        //    string root = string.Format("{0}://{1}{2}",
        //                                Uri.Scheme,
        //                                Uri.Host,
        //                                Uri.Port == 80
        //                                    ? string.Empty
        //                                    : ":" + Uri.Port);

        //    if (!root.EndsWith("/"))
        //        root += "/";

        //    return new Uri(root);
        //}

        //public override string ToString()
        //{
        //    return Uri.AbsoluteUri;
        //}
    }
}
