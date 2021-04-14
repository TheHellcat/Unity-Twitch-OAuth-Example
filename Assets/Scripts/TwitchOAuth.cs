/*
 * Simple Twitch OAuth flow example
 * by HELLCAT
 *
 * At first glance, this looks like more than it actually is.
 * It's really no rocket science, promised! ;-)
 * And for any further questions contact me directly or on the Twitch-Developers discord.
 *
 * 🐦 https://twitter.com/therealhellcat
 * 📺 https://www.twitch.tv/therealhellcat
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class TwitchOAuth : MonoBehaviour
{
    [SerializeField] private string twitchAuthUrl = "https://id.twitch.tv/oauth2/authorize";
    [SerializeField] private string twitchClientId = "PUT YOUR CLIENT ID HERE";
    [SerializeField] private string twitchClientSecret = "PUT YOUR CLIENT SECRET HERE";
    [SerializeField] private string twitchRedirectUrl = "http://localhost:8080/";
    [SerializeField] private TwitchApiCallHelper twitchApiCallHelper = null;
    [SerializeField] private TextMeshProUGUI uiTextToken = null;
    private string _twitchAuthStateVerify;
    private string _authToken;

    private void Start()
    {
        _authToken = "";
        UpdateTokenDisplay();
        StartCoroutine(DisplayUpdater());
    }

    /// <summary>
    /// Starts the Twitch OAuth flow by constructing the Twitch auth URL based on the scopes you want/need.
    /// </summary>
    public void InitiateTwitchAuth()
    {
        string[] scopes;
        string s;

        // list of scopes we want
        scopes = new[]
        {
            "user:read:email",
            "chat:edit",
            "chat:read",
            "channel:read:redemptions",
            "channel_subscriptions",
            "user:read:broadcast",
            "user:edit:broadcast",
            "channel:manage:redemptions"
        };

        // generate something for the "state" parameter.
        // this can be whatever you want it to be, it's gonna be "echoed back" to us as is and should be used to
        // verify the redirect back from Twitch is valid.
        _twitchAuthStateVerify = ((Int64) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

        // query parameters for the Twitch auth URL
        s = "client_id=" + twitchClientId + "&" +
            "redirect_uri=" + UnityWebRequest.EscapeURL(twitchRedirectUrl) + "&" +
            "state=" + _twitchAuthStateVerify + "&" +
            "response_type=code&" +
            "scope=" + String.Join("+", scopes);

        // start our local webserver to receive the redirect back after Twitch authenticated
        StartLocalWebserver();

        // open the users browser and send them to the Twitch auth URL
        Application.OpenURL(twitchAuthUrl + "?" + s);
    }

    /// <summary>
    /// Opens a simple "webserver" like thing on localhost:8080 for the auth redirect to land on.
    /// Based on the C# HttpListener docs: https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener
    /// </summary>
    private void StartLocalWebserver()
    {
        HttpListener httpListener = new HttpListener();

        httpListener.Prefixes.Add(twitchRedirectUrl);
        httpListener.Start();
        httpListener.BeginGetContext(new AsyncCallback(IncomingHttpRequest), httpListener);
    }

    /// <summary>
    /// Handles the incoming HTTP request
    /// </summary>
    /// <param name="result"></param>
    private void IncomingHttpRequest(IAsyncResult result)
    {
        string code;
        string state;
        HttpListener httpListener;
        HttpListenerContext httpContext;
        HttpListenerRequest httpRequest;
        HttpListenerResponse httpResponse;
        string responseString;

        // get back the reference to our http listener
        httpListener = (HttpListener) result.AsyncState;

        // fetch the context object
        httpContext = httpListener.EndGetContext(result);

        // if we'd like the HTTP listener to accept more incoming requests, we'd just restart the "get context" here:
        // httpListener.BeginGetContext(new AsyncCallback(IncomingHttpRequest),httpListener);
        // however, since we only want/expect the one, single auth redirect, we don't need/want this, now.
        // but this is what you would do if you'd want to implement more (simple) "webserver" functionality
        // in your project.

        // the context object has the request object for us, that holds details about the incoming request
        httpRequest = httpContext.Request;

        code = httpRequest.QueryString.Get("code");
        state = httpRequest.QueryString.Get("state");

        // check that we got a code value and the state value matches our remembered one
        if ((code.Length > 0) && (state == _twitchAuthStateVerify))
        {
            // if all checks out, use the code to exchange it for the actual auth token at the API
            GetTokenFromCode(code);
        }

        // build a response to send an "ok" back to the browser for the user to see
        httpResponse = httpContext.Response;
        responseString = "<html><body><b>DONE!</b><br>(You can close this tab/window now)</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        // send the output to the client browser
        httpResponse.ContentLength64 = buffer.Length;
        System.IO.Stream output = httpResponse.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();

        // the HTTP listener has served it's purpose, shut it down
        httpListener.Stop();
        // obv. if we had restarted the waiting for more incoming request, above, we'd not Stop() it here.
    }

    /// <summary>
    /// Makes the API call to exchange the received code for the actual auth token
    /// </summary>
    /// <param name="code">The code parameter received in the callback HTTP reuqest</param>
    private void GetTokenFromCode(string code)
    {
        string apiUrl;
        string apiResponseJson;
        ApiCodeTokenResponse apiResponseData;

        // construct full URL for API call
        apiUrl = "https://id.twitch.tv/oauth2/token" +
                 "?client_id=" + twitchClientId +
                 "&client_secret=" + twitchClientSecret +
                 "&code=" + code +
                 "&grant_type=authorization_code" +
                 "&redirect_uri=" + UnityWebRequest.EscapeURL(twitchRedirectUrl);

        // make sure our API helper knows our client ID (it needed for the HTTP headers)
        twitchApiCallHelper.TwitchClientId = twitchClientId;

        // make the call!
        apiResponseJson = twitchApiCallHelper.CallApi(apiUrl, "POST");

        // parse the return JSON into a more usable data object
        apiResponseData = JsonUtility.FromJson<ApiCodeTokenResponse>(apiResponseJson);

        // fetch the token from the response data
        _authToken = apiResponseData.access_token;
    }

    /// <summary>
    /// Updates the UI in the scene with the current auth data
    /// </summary>
    private void UpdateTokenDisplay()
    {
        uiTextToken.text = _authToken;
    }

    /// <summary>
    /// This is a small necessary evil, as we can't update the UI directly from the HTTP handler.
    /// It's a Unity thing and has to do with the fact that internally the HTTP handler runs on a separate thread.
    /// Nothing you need to worry about. Just Unity stuff, lol.
    /// </summary>
    /// <returns></returns>
    IEnumerator DisplayUpdater()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            UpdateTokenDisplay();
        }
    }
}
