using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using WebService.com.softexpert.tecfy;

namespace TecnoSet.Ecm.Wpf.Services.SE
{
    public class SEClient: Documento
    {
        private String m_HeaderName; private String m_HeaderValue;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);

            if (null != this.m_HeaderName)
                request.Headers.Add(this.m_HeaderName, this.m_HeaderValue);
            return (WebRequest)request;
        }

        public SEClient(string urlWebService, string usaurio, string senha) : base(urlWebService)
        {

        }

        public SEClient(string urlWebService) : base(urlWebService)
        {
        }

        public void SetRequestHeader(String headerName, String headerValue)
        {
            this.m_HeaderName = headerName;
            this.m_HeaderValue = headerValue;
        }

        public void SetAuthentication(string userName, string password)
        {
            string usernamePassword = userName + ":" + password;
            this.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));
        }


    }
}
