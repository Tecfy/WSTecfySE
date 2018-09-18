using System.Web.Configuration;
using TecnoSet.Ecm.Wpf.Services.SE;

namespace WebService.Connection
{
    public static class SEConnection
    {
        readonly static string Username = WebConfigurationManager.AppSettings["Username"];
        readonly static string Password = WebConfigurationManager.AppSettings["Password"];
        readonly static string URL = WebConfigurationManager.AppSettings["Url"];

        public static SEClient GetConnection()
        {
            SEClient seClient = new SEClient
            {
                Url = URL
            };
            seClient.SetAuthentication(Username, Password);

            return seClient;
        }
    }
}