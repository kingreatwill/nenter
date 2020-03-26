using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Nenter.HttpClient
{
    public class Interceptor : DelegatingHandler
    {
        /// <summary>
        /// Occurs before a HTTP request sending.
        /// </summary>
        public event EventHandler<InterceptorEventArgs> BeforeSend;

        /// <summary>
        /// Occurs after received a response of a HTTP request. (include it wasn't succeeded.)
        /// </summary>
        public event EventHandler<InterceptorEventArgs> AfterSend;

        public Interceptor()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = default(HttpResponseMessage);
           
            this.BeforeSend?.Invoke(this, new InterceptorEventArgs(request, response));

            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            this.AfterSend?.Invoke(this, new InterceptorEventArgs(request, response));

            return response;
        }
       
    }
}
