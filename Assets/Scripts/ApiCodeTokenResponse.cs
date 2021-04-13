/*
 * Simple Twitch OAuth flow example
 * by HELLCAT
 *
 * 🐦 https://twitter.com/therealhellcat
 * 📺 https://www.twitch.tv/therealhellcat
 */

using System;

/// <summary>
/// Data object to parse API response for auth token into
/// </summary>
[Serializable]
public class ApiCodeTokenResponse
{
    public string access_token;
    public int expires_in;
    public string refresh_token;
    public string[] scope;
    public string token_type;
}
