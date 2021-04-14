# Unity3D - Twitch OAuth Example

----------

A quick example of how to do Twitch OAuth using a local "webserver" to receive the redirect back from the Twicth authentication flow, completely in Unity, without any backends or other external tools.

## How to Run This Example

### To do Twitch side

- If you haven't done so already, head to https://dev.twitch.tv/ and create an application in your developer console
- Go to the details of your application and note your client-ID and the client-secret

### To do Unity side

- Open the project in Unity
- In Unity, open the scene `Scenes/SampleScene`
- Click on the `TwitchAuth` gameobject in the hierarchy of the scene
- Set the values `Twitch Clien ID` and `Twitch Client Secret` to the ones you noted above on the Twitch side
- Hit `PLAY`
- Click the `Authenticate with Twitch` button in the game scene.

## I have questions

Feel free to hit me up on the Twitch-Developers discord, or my Tweeter:
https://twitter.com/therealhellcat
