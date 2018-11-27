using RestSharp;

namespace WebService.Connection
{
    public class RestRequestHelper
    {
        public static RestRequest Get(Method method, string json = null)
        {
            var request = new RestRequest(method);
            request.AddHeader("content-type", "application/json");
            if (method == Method.GET)
            {
                request.AddParameter("application/json", "{}", ParameterType.RequestBody);
            }
            else if (method == Method.POST || method == Method.PUT)
            {
                request.AddParameter("application/json", json, ParameterType.RequestBody);
            }

            return request;
        }
    }
}