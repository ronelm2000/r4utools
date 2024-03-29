﻿using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Scripting;
using Flurl.Http;
using Montage.RebirthForYou.Tools.CLI.Utilities.Components;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Utilities
{
    public static class UriExtensions
    {
        static readonly HttpClient client = new HttpClient();
        static readonly IFlurlClient rateLimitingClient = new FlurlClient() //
            .Configure(se =>
            {
                se.HttpClientFactory = new HTTPRateHandlerHttpClientFactory(new RateLimiterOptions { MaxTries = 10, Rate = 500, PerSecond = 1 });
            });
        //static readonly Dictionary<string, HTTPRateHandler> siteHandlers = new Dictionary<string, HTTPRateHandler>();

        public static async Task<IDocument> DownloadHTML(this Uri uri)
        {
            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
            //var response = myReq.GetResponse();
            //Log.Information("Response Content Length: {ContentLength}", response.ContentLength);
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                var values = new NameValueCollection();
                //values.Add("Referer", "http://www.nseindia.com/products/content/equities/equities/bulk.htm");
                values.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36");
                values.Add("X-Requested-With", "XMLHttpRequest");
                values.Add("Accept", "*/*");
                values.Add("Accept-Language", "en-US,en;q=0.8");
                wc.Headers.Add(values);

                var content = wc.DownloadString(uri);

                var config = Configuration.Default.WithDefaultLoader()
                        .WithCss()
                        ;
                var context = BrowsingContext.New(config);
                return await context.OpenAsync(req => req.Content(content));
            }
        }

        public static async Task<IDocument> DownloadHTML(this Uri uri, params (string Key, string Value)[] keyValuePairs)
        {
            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
            //var response = myReq.GetResponse();
            //Log.Information("Response Content Length: {ContentLength}", response.ContentLength);
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;

                var values = new NameValueCollection();
                //values.Add("Referer", "http://www.nseindia.com/products/content/equities/equities/bulk.htm");
                values.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36");
                values.Add("X-Requested-With", "XMLHttpRequest");
                values.Add("Accept", "*/*");
                values.Add("Accept-Language", "en-US,en;q=0.8");
                foreach (var kvp in keyValuePairs)
                {
                    values.Remove(kvp.Key);
                    values.Add(kvp.Key, kvp.Value);
                }
                wc.Headers.Add(values);

                var content = wc.DownloadString(uri);
                var config = Configuration.Default.WithDefaultLoader()
                        .WithCss()
                        ;
                var context = BrowsingContext.New(config);
                return await context.OpenAsync(req =>
                {
                    req.Content(content);
                    req.Address(uri);
                });
            }
        }

        public static IFlurlRequest WithReferrer(this string urlString, string referrerUrl)
        {
            return urlString.AllowAnyHttpStatus().WithReferrer(referrerUrl);
        }

        public static IFlurlRequest WithReferrer(this Flurl.Url urlString, string referrerUrl)
        {
            return urlString.AllowAnyHttpStatus().WithReferrer(referrerUrl);
        }

        public static IFlurlRequest WithReferrer(this IFlurlRequest request, string referrerUrl)
        {
            return request.WithHeader("Referer", referrerUrl);
        }

        public static IFlurlRequest WithRateLimiter(this IFlurlRequest request)
        {
            return request.WithClient(rateLimitingClient);
        }

        public static IFlurlRequest WithRESTHeaders(this string urlString)
        {
            return urlString.WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36")
                            .WithHeader("Accept", "text/plain");
        }

        public static IFlurlRequest WithHTMLHeaders(this string urlString)
        {
            return urlString.WithHeaders(new 
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                Accept = "*/*",
                Accept_Language = "en-US,en;q=0.8"
            });
        }

        public static IFlurlRequest WithHTMLHeaders(this IFlurlRequest req)
        {
            return req.WithHeaders(new
            {
                User_Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.118 Safari/537.36",
                Accept = "*/*",
                Accept_Language = "en-US,en;q=0.8"
            });
        }

        public static IFlurlRequest WithImageHeaders(this Uri url)
        {
            return url  .AbsoluteUri
                        .WithTimeout(TimeSpan.FromSeconds(45));
        }

        public static async Task<IDocument> GetHTMLAsync(this IFlurlRequest flurlReq)
        {
            //var content = wc.DownloadString(uri);
            var config = Configuration.Default.WithDefaultLoader()
                    .WithCss()
                    ;
            var context = BrowsingContext.New(config);
            var stream = await flurlReq.GetStreamAsync();
            return await context.OpenAsync(req =>
            {
                req.Address(flurlReq.Url.ToString());
                req.Content(stream, true);
            });

        }

        public static async Task<IDocument> RecieveHTML(this HttpResponseMessage flurlReq)
        {
            var config = Configuration.Default.WithDefaultLoader()
                    .WithCss()
                    //.With(I)
                    ;
            var context = BrowsingContext.New(config);
            var stream = await flurlReq.Content.ReadAsStreamAsync(); //.ReceiveStream();
            return await context.OpenAsync(req =>
            {
                req.Address(flurlReq.RequestMessage.RequestUri.AbsoluteUri);
                req.Content(stream, true);
            });
        }

    }
}
