using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Dynamic;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace CdekExample;

public interface ICdekService
{
    public CdekResponse GetOffices(Dictionary<string, object> requestParams);
    public CdekResponse Calculate(Dictionary<string, object> requestParams);
}

internal class CdekService: ICdekService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string apiUrl;
    private readonly string clientId;
    private readonly string clientSecret;

    public CdekService(IConfigurationRoot config, IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
        this.apiUrl = config["CdekApiUrl"];
        this.clientId = config["CdekClientId"];
        this.clientSecret = config["CdekClientSecret"];
    }

    public CdekResponse GetOffices(Dictionary<string, object> requestParams)
    {
        try
        {
            string token = this.getAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var result = this.httpRequest("deliverypoints", requestParams, token, false, false, true);
            return result;
        }
        catch (Exception x)
        {
            return null;
        }
    }

    public CdekResponse Calculate(Dictionary<string, object> requestParams)
    {
        try
        {
            string token = this.getAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var result = this.httpRequest("calculator/tarifflist", requestParams, token, false, true, false);
            return result;
        }
        catch (Exception x)
        {
            return null;
        }
    }

    private string getAuthToken()
    {
        var data = new Dictionary<string, object>();
        data["grant_type"] = "client_credentials";
        data["client_id"] = this.clientId;
        data["client_secret"] = this.clientSecret;

        var result = this.httpRequest("/oauth/token", data, null, true, false, false);
        return result?.Data.access_token?.ToString();
    }

    private CdekResponse httpRequest(string method, Dictionary<string, object> requestParams, string authToken,
        bool useFormData, bool useJson, bool expectList)
    {
        // for get requests append query string
        string reqUrl = $"{this.apiUrl}/{method}";
        using (var req = new HttpRequestMessage(
                   useFormData || useJson ? HttpMethod.Post : HttpMethod.Get,
                   useFormData || useJson ? reqUrl : QueryHelpers.AddQueryString(reqUrl, requestParams.ToDictionary(k => k.Key, k => k.Value?.ToString()))))
        {
            if (useFormData)
            {
                req.Content = new FormUrlEncodedContent(requestParams.ToDictionary(k => k.Key, k => k.Value?.ToString()));
            }
            else if (useJson)
            {
                req.Content = new StringContent(JsonConvert.SerializeObject(requestParams), Encoding.UTF8, "application/json");
            }
            req.Headers.TryAddWithoutValidation("Accept", "application/json");
            req.Headers.TryAddWithoutValidation("X-App-Name", "widget_pvz");
            req.Headers.TryAddWithoutValidation("X-App-Version", "3.11.1");
            req.Headers.TryAddWithoutValidation("User-Agent", "widget/3.11.1");
            // authenticate request if token was passed
            if (!string.IsNullOrEmpty(authToken))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }

            using (var res = this.httpClientFactory.CreateClient().Send(req))
            {
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                dynamic result = expectList ?
                    res.Content.ReadFromJsonAsync<IEnumerable<ExpandoObject>>().GetAwaiter().GetResult() :
                    res.Content.ReadFromJsonAsync<ExpandoObject>().GetAwaiter().GetResult();

                // collect custom headers (e.g. X-Current-Page, X-Total-Elements, X-Total-Pages)
                var headers = res.Headers?.Where(kvp => kvp.Key.ToLower().StartsWith("x-"))
                    .SelectMany(kvp => kvp.Value?.Select(v => (name: kvp.Key, value: v))).ToList();
                headers.Add(("X-Service-Version", "3.11.1"));

                // return both data and custom headers
                return new CdekResponse
                {
                    Data = result,
                    Headers = headers
                };
            }
        }
    }
}