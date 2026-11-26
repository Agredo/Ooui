using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ooui.AspNetCore
{
    public class ElementResult : ActionResult
    {
        readonly Element element;
        readonly string title;
        readonly bool disposeAfterSession;
        readonly ILogger logger;
        string bodyHeaderHtml;

        public ElementResult (Element element, string title = "", bool disposeAfterSession = true, ILogger logger = null, string bodyHeaderHtml = null)
        {
            this.logger = logger;
            this.element = element;
            this.title = title;
            this.disposeAfterSession = disposeAfterSession;
            this.bodyHeaderHtml = bodyHeaderHtml;
        }

        /// <summary>
        /// Gets or sets the content that is rendered above the page for this response only.
        /// When it is <c>null</c> the application wide <see cref="UI.BodyHeaderHtml"/> is used.
        /// </summary>
        public string BodyHeaderHtml {
            get => bodyHeaderHtml;
            set => bodyHeaderHtml = value;
        }

        public override async Task ExecuteResultAsync (ActionContext context)
        {
            var httpContext = context.HttpContext;
            var response = httpContext.Response;
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "text/html; charset=utf-8";
            response.Headers.CacheControl = "no-cache, no-store, must-revalidate";

            if (element.WantsFullScreen) {
                element.Style.Width = GetCookieDouble (httpContext.Request.Cookies, "oouiWindowWidth", 32, 640, 10000);
                element.Style.Height = GetCookieDouble (httpContext.Request.Cookies, "oouiWindowHeight", 24, 480, 10000);
            }

            var sessionId = WebSocketHandler.BeginSession (httpContext, element, disposeAfterSession, logger);
            var initialHtml = element.OuterHtml;
            var html = UI.RenderTemplate (WebSocketHandler.WebSocketPath + "?id=" + sessionId, title: title, initialHtml: initialHtml, bodyHeaderHtml: bodyHeaderHtml);
            var htmlBytes = Encoding.UTF8.GetBytes (html);
            response.ContentLength = htmlBytes.Length;
            await response.Body.WriteAsync (htmlBytes.AsMemory ()).ConfigureAwait (false);
        }

        static double GetCookieDouble (IRequestCookieCollection cookies, string key, double min, double def, double max)
        {
            if (cookies.TryGetValue (key, out var s)) {
                if (double.TryParse (s, out var d)) {
                    if (d < min) return min;
                    if (d > max) return max;
                    return d;
                }
                return def;
            }
            else {
                return def;
            }
        }
    }
}
