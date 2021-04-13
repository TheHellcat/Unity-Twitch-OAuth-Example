/*
 * Simple Twitch OAuth flow example
 * by HELLCAT
 *
 * 🐦 https://twitter.com/therealhellcat
 * 📺 https://www.twitch.tv/therealhellcat
 */

using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Small helper class to help with API things
/// </summary>
public class TwitchApiCallHelper : MonoBehaviour
{
    private HttpClient _httpClient = new HttpClient();
    private string _twitchClientId;
    private string _twitchAuthToken;

    public string TwitchClientId
    {
        set { _twitchClientId = value; }
    }

    public string TwitchAuthToken
    {
        set { _twitchAuthToken = value; }
    }

    private void Start()
    {
        _twitchClientId = "";
        _twitchAuthToken = "";
    }

    /// <summary>
    /// Simple helper method to call a Twitch API endpoint and return its response data.
    /// </summary>
    /// <remarks>
    /// This BLOCKS the current thread till the API response has been recieved, so in a production environment you
    /// either want a different/async method or simply run this in an own thread/job/task.
    /// </remarks>
    /// <param name="endpoint">API endpoint to call (full URL)</param>
    /// <param name="method">(optional) HTTP method to use - defaults to GET</param>
    /// <param name="body">(optional) Any body data to send</param>
    /// <param name="headers">(optional) Any additional headers (the standard API headers are always added)</param>
    /// <returns>Returns the response body of the API call, if any was recieved.</returns>
    public string CallApi(string endpoint, string method = "GET", string body = "", string[] headers = null)
    {
        HttpRequestMessage httpRequest;
        string[] headerParts;
        string returnData;

        // init some things
        _httpClient.BaseAddress = null;
        _httpClient.DefaultRequestHeaders.Clear();

        // set http client method as requested
        switch (method.ToLower())
        {
            case "get":
                httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
                break;

            case "post":
                httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
                break;

            case "put":
                httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint);
                break;

            case "patch":
                httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint);
                break;

            case "delete":
                httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                break;

            default:
                httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
                break;
        }

        // set http client request body, if any was supplied
        if (body.Length > 0)
        {
            httpRequest.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        // set default headers
        if (_twitchAuthToken.Length > 0)
        {
            httpRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _twitchAuthToken);
        }

        if (_twitchClientId.Length > 0)
        {
            httpRequest.Headers.TryAddWithoutValidation("Client-Id", _twitchClientId);
        }

        httpRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");

        // set additional headers, if any were supplied
        if (headers != null)
        {
            foreach (string header in headers)
            {
                headerParts = header.Split(':');
                if (headerParts.Length >= 2)
                {
                    if (headerParts[1] != "")
                    {
                        httpRequest.Headers.TryAddWithoutValidation(headerParts[0].Trim(), headerParts[1].Trim());
                    }
                }
            }
        }

        // send request and wait for it to complete
        Task<HttpResponseMessage> httpRespose = _httpClient.SendAsync(httpRequest);
        while (!httpRespose.IsCompleted)
        {
            // NOP - keep waiting....
        }

        // fetch response content
        Task<string> httpResponseContent = httpRespose.Result.Content.ReadAsStringAsync();
        while (!httpResponseContent.IsCompleted)
        {
            // NOP - keep waiting....
        }

        // return the response content and be done - calling an API can be so easy :-D
        returnData = httpResponseContent.Result;
        return returnData;
    }
}
