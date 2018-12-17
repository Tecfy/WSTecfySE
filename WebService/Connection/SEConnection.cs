using System.Net;
using System.Web.Configuration;
using TecnoSet.Ecm.Wpf.Services.SE;

namespace WebService.Connection
{
    public static class SEConnection
    {
        readonly static string Username = WebConfigurationManager.AppSettings["Username"];
        readonly static string Password = WebConfigurationManager.AppSettings["Password"];
        readonly static string URL = WebConfigurationManager.AppSettings["Url"];
        readonly static string URLAdm = WebConfigurationManager.AppSettings["UrlAdm"];

        public static SEClient GetConnection()
        {
            SEClient seClient = new SEClient { Url = URL };
            seClient.SetAuthentication(Username, Password);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            return seClient;
        }

        public static SEAdministration GetConnectionAdm()
        {
            SEAdministration seAdministration = new SEAdministration { Url = URLAdm };
            seAdministration.SetAuthentication(Username, Password);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            return seAdministration;
        }
    }
}