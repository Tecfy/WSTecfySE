using System;
using System.Net;
using System.Text;
using WebService.com.softexpert.tecfy;

namespace TecnoSet.Ecm.Wpf.Services.SE
{
    public class SEClient: Documento
    {
        private string m_HeaderName; private string m_HeaderValue;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);

            if (null != this.m_HeaderName)
                request.Headers.Add(this.m_HeaderName, this.m_HeaderValue);
            return (WebRequest)request;
        }

        public void SetRequestHeader(string headerName, string headerValue)
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
