using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Nenter.Blog.Application.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var p = context.Request.Path;
//            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
//            {
//                await _next(context);
//                return;
//            }
//            if (
//                string.IsNullOrWhiteSpace(context.Request.Query["token"].FirstOrDefault())
//                || string.IsNullOrWhiteSpace(context.Request.Cookies[Extensions.AuthorizeName])
//            )
//            {
//                if (context.Request.IsAjaxRequest())
//                {
//                    throw new Exception("请登录.");
//                }
//                context.Response.Redirect(conf.Value.LoginUrl);
//            }
//            else
            {
                await _next(context); 
            }
        }
    }
}