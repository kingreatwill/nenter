using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Nenter.HttpClient
{
    public class InterceptorEventArgs : EventArgs
    {
        public HttpRequestMessage Request { get; }

        public HttpResponseMessage Response { get; }

        public InterceptorEventArgs(HttpRequestMessage request, HttpResponseMessage response)
        {
            this.Request = request;
            this.Response = response;
        }
    }
}
