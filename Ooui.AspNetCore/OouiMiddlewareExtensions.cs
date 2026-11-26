using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ooui.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class OouiMiddlewareExtensions
    {
        public static IApplicationBuilder UseOoui (this IApplicationBuilder app, string jsPath = "/ooui.js", string webSocketPath = "/ooui.ws", TimeSpan? sessionTimeout = null)
        {
            if (string.IsNullOrWhiteSpace (webSocketPath))
                throw new ArgumentException ("A path to be used for Ooui web sockets must be specified", nameof (webSocketPath));
            
            if (string.IsNullOrWhiteSpace (jsPath))
                throw new ArgumentException ("A path to be used for Ooui JavaScript must be specified", nameof (jsPath));

            WebSocketHandler.WebSocketPath = webSocketPath;

            if (sessionTimeout.HasValue) {
                WebSocketHandler.SessionTimeout = sessionTimeout.Value;
            }

            var webSocketOptions = new WebSocketOptions () {
                KeepAliveInterval = WebSocketHandler.SessionTimeout,
            };
            app.UseWebSockets (webSocketOptions);

            Ooui.UI.ServerEnabled = false;

            app.Use (async (context, next) =>
            {
                if (context.Request.Path == jsPath) {
                    await WriteCachedContentAsync (context, Ooui.UI.ClientJsBytes, Ooui.UI.ClientJsEtag, "application/javascript; charset=utf-8").ConfigureAwait (false);
                }
                else if (context.Request.Path == WebSocketHandler.WebSocketPath) {
                    if (context.WebSockets.IsWebSocketRequest) {
                        await WebSocketHandler.HandleWebSocketRequestAsync (context).ConfigureAwait (false);
                    }
                    else {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    }
                }
                else if (Ooui.UI.TryGetFileContentAtPath (context.Request.Path, out var file)) {
                    await WriteCachedContentAsync (context, file.Content, file.Etag, file.ContentType).ConfigureAwait (false);
                }
                else {
                    await next ().ConfigureAwait (false);
                }
            });

            return app;
        }

        static async Task WriteCachedContentAsync (HttpContext context, byte[] content, string etag, string contentType)
        {
            var response = context.Response;

            if (!string.IsNullOrEmpty (etag) && string.Equals (context.Request.Headers.IfNoneMatch, etag, StringComparison.Ordinal)) {
                response.StatusCode = StatusCodes.Status304NotModified;
                return;
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = contentType;
            response.ContentLength = content.Length;
            response.Headers.CacheControl = "public, max-age=60";
            if (!string.IsNullOrEmpty (etag)) {
                response.Headers.ETag = etag;
            }

            await response.Body.WriteAsync (content.AsMemory ()).ConfigureAwait (false);
        }
    }
}
