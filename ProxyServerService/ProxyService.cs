using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace ProxyServerService
{
    public class ProxyService
    {
        private ProxyServer _proxyServer;

        public bool Start()
        {
            _proxyServer = new ProxyServer();
            _proxyServer.BeforeRequest += OnRequest;
            _proxyServer.BeforeResponse += OnResponse;
            _proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            _proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            var port = 8888;//colocar no arquivo de configuração;
            var enableSsl = true; //colocar no arquivo de configuração;

            _proxyServer.AddEndPoint(new ExplicitProxyEndPoint(IPAddress.Any, port, enableSsl));
            _proxyServer.Start();

            return true;
        }

        private async Task OnRequest(object sender, SessionEventArgs e)
        {

            //read request headers
            var requestHeaders = e.WebSession.Request.RequestHeaders;

            if ((e.WebSession.Request.Method == "POST" || e.WebSession.Request.Method == "PUT"))
            {
                //Get/Set request body bytes
                byte[] bodyBytes = await e.GetRequestBody();
                await e.SetRequestBody(bodyBytes);

                //Get/Set request body as string
                string bodyString = await e.GetRequestBodyAsString();
                await e.SetRequestBodyString(bodyString);

            }

            //To cancel a request with a custom HTML content
            //Filter URL
            if (e.WebSession.Request.RequestUri.AbsoluteUri.Contains("google.com"))
            {
                await e.Ok("<!DOCTYPE html>" +
                      "<html><body><h1>" +
                      "Website Blocked" +
                      "</h1>" +
                      "<p>Blocked by titanium web proxy.</p>" +
                      "</body>" +
                      "</html>");
            }
            //Redirect example
            if (e.WebSession.Request.RequestUri.AbsoluteUri.Contains("wikipedia.org"))
            {
                await e.Redirect("https://www.paypal.com");
            }
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            //read response headers
            //var responseHeaders = e.WebSession.Response.ResponseHeaders;

            //if (!e.ProxySession.Request.Host.Equals("medeczane.sgk.gov.tr")) return;
            if (e.WebSession.Request.Method == "GET" || e.WebSession.Request.Method == "POST")
            {
                if (e.WebSession.Response.ResponseStatusCode == "200")
                {
                    if (e.WebSession.Response.ContentType != null && e.WebSession.Response.ContentType.Trim().ToLower().Contains("text/html"))
                    {
                        //byte[] bodyBytes = await e.GetResponseBody();
                        //await e.SetResponseBody(bodyBytes);

                        //string body = await e.GetResponseBodyAsString();
                        //await e.SetResponseBodyString(body);
                    }
                }
            }
        }

        private Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            //set IsValid to true/false based on Certificate Errors
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                e.IsValid = true;

            return Task.FromResult(0);
        }

        private Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
        {
            //set e.clientCertificate to override
            return Task.FromResult(0);
        }

        public bool Stop()
        {
            _proxyServer.BeforeRequest -= OnRequest;
            _proxyServer.BeforeResponse -= OnResponse;
            _proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
            _proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;

            _proxyServer.Stop();
            _proxyServer.Dispose();
            return true;
        }
    }
}
