using System;
using System.Collections.Generic;
using System.Text;

namespace Nenter.HttpClient
{
    class InterceptorExtension
    {
        //public static void AddHttpClientInterceptor(this IServiceCollection services)
        //{
        //    // 1
        //    services.TryAddSingleton(serviceProvider =>
        //    {
        //        var httpClient = serviceProvider.GetRequiredService<HttpClient>();
        //        var handlerField = typeof(HttpMessageInvoker).GetField("_handler", BindingFlags.Instance | BindingFlags.NonPublic);
        //        var baseHandler = handlerField?.GetValue(httpClient) as HttpMessageHandler;

        //        var interceptor = new HttpClientInterceptor(baseHandler);
        //        if (handlerField != null && baseHandler != null)
        //            handlerField.SetValue(httpClient, interceptor);

        //        return interceptor;
        //    });

        //    // 2
        //    //services.AddHttpClient<ProductAdapter>(
        //    //    x =>
        //    //    {
        //    //        x.BaseAddress = new Uri("http://localhost:5000");
        //    //        x.DefaultRequestHeaders.Add("User-Agent", "RequestInterceptorTests");
        //    //    }).AddHttpMessageHandler<RequestsInterceptor>();
        //}
    }
}
