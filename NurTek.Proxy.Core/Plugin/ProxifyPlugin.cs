using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NurTek.Proxy.Core.Helpers;
using NurTek.Proxy.Core.Http;

namespace NurTek.Proxy.Core.Plugin
{
    public class ProxifyPlugin : AbstractPlugin
    {
        static readonly List<string> _doNotTouch = new List<string> { "text/javascript", "application/javascript", "application/x-javascript", "text/plain" };
        private readonly IProxyEvents _proxyEvents;
        public ProxifyPlugin(IProxyEvents proxyEvents) : base(proxyEvents)
        {
            _proxyEvents = proxyEvents;
            proxyEvents.onBeforeRequest += ProxyEvents_onBeforeRequest;
            proxyEvents.onCompleted += ProxyEvents_onCompleted;
        }

        private void ProxyEvents_onBeforeRequest(ProxyEventArgs evt)
        {
            var request = evt.ProxyParams.Contents["request"] as Request;

            if (string.IsNullOrEmpty(request.Post["convertGET"])) return;
            request.Post.Remove("convertGET");
            foreach (string o in request.Post)
            {
                request.Get.Set(o, request.Post[o]);
            }
            request.Post.Clear();
            request.Method = HttpMethod.Get;
        }

        private void ProxyEvents_onCompleted(ProxyEventArgs evt)
        {
            var response = evt.ProxyParams.Contents["response"] as Response;
            var contentType = response.Headers.Get("content-type");

            if (_doNotTouch.Any(v => v.Equals(contentType)))
                return;

            var str = response.Content;
            const string iframePattern = @"<iframe[^>]*>[^<]*<\\/iframe>";
            str = Regex.Replace(str, iframePattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            str = ProxifyHead(str);
            str = ProxifyCss(str);

            const string htmlAttrPattern = @"(?:src|href)\s*=\s*(["" |\'])(.*?)\1";
            str = Regex.Replace(str, htmlAttrPattern, CallBackHtmlAttr, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            const string formAction = @"<form[^>]*action=([""\'])(.*?)\1[^>]*>";
            str = Regex.Replace(str, formAction, CallBackFormAction, RegexOptions.IgnoreCase);

            response.Content = str;

        }

        private string CallBackHtmlAttr(Match match)
        {
            var url = match.Groups[2].Value.Trim();

            if (string.IsNullOrWhiteSpace(url))
                return match.Groups[0].Value;

            if (url.StartsWith("data:", StringComparison.CurrentCultureIgnoreCase) ||
                url.StartsWith("magnet:", StringComparison.CurrentCultureIgnoreCase))
                return match.Groups[0].Value;

            return match.Groups[0].Value.Replace(url, Helper.ProxifyUrl(url, _proxyEvents.BaseURL, _proxyEvents.AppURL));
        }

        private string CallBackFormAction(Match match)
        {
            var url = match.Groups[2].Value;

            if (string.IsNullOrWhiteSpace(url))
                url = _proxyEvents.BaseURL;

            var newAction = Helper.ProxifyUrl(url, _proxyEvents.BaseURL, _proxyEvents.AppURL);
            const string formPostPattern = @"method=([""\'])post\1";
            var formPost = Regex.IsMatch(match.Groups[0].Value, formPostPattern, RegexOptions.IgnoreCase);

            var result = match.Groups[0].Value.Replace(url, newAction);

            if (!formPost)
            {
                // may throw Duplicate Attribute warning but only first method matters
                result = result.Replace("<form", "<form method=\"POST\"");
                result += "<input type=\"hidden\" name=\"convertGET\" value=\"1\">";
            }

            return result;
        }

        private string CallBackMetaRefresh(Match match)
        {
            var url = match.Groups[2].Value;
            return match.Groups[0].Value.Replace(url, Helper.ProxifyUrl(url, _proxyEvents.BaseURL, _proxyEvents.AppURL));
        }

        private string CallBackCssUrl(Match match)
        {
            var url = match.Groups[1].Value.Trim();
            if (url.StartsWith("data:", StringComparison.CurrentCultureIgnoreCase))
                return match.Groups[0].Value;

            return match.Groups[0].Value.Replace(match.Groups[1].Value,
                Helper.ProxifyUrl(url, _proxyEvents.BaseURL, _proxyEvents.AppURL));
        }

        private string CallBackCssImport(Match match)
        {
            return match.Groups[0].Value.Replace(match.Groups[2].Value,
                Helper.ProxifyUrl(match.Groups[2].Value, _proxyEvents.BaseURL, _proxyEvents.AppURL));
        }

        private string ProxifyHead(string content)
        {
            const string metaRefreshPattern = @"/content=([""\'])\d+\s*;\s*url=(.*?)\1";
            var str = Regex.Replace(content, metaRefreshPattern, CallBackMetaRefresh, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return str;
        }

        private string ProxifyCss(string content)
        {
            const string cssUrlPattern = @"[^a-z]{1}url\s*\((?:\'|""|)(.*?)(?:\'|""|)\)";
            content = Regex.Replace(content, cssUrlPattern, CallBackCssUrl, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            const string cssImportPattern = @"@import (\'|"")(.*?)\1";
            content = Regex.Replace(content, cssImportPattern, CallBackCssImport, RegexOptions.IgnoreCase);

            return content;
        }
    }
}
