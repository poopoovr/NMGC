using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NMGC.Version_Checking;

public static class VersionCheckingInitializer
{
    public static Action VersionOutdatedDetected;
    public static Action VersionNotLatestDetected;

    public static bool VersionOutdated;
    public static bool VersionNotLatest;

    public static string OutdatedMessage;
    public static string NotLatestMessage;

    public static Version LatestVersion;

    public static void StartVersionChecking()
    {
        using HttpClient    httpClient   = new();
        HttpResponseMessage dataResponse = httpClient.GetAsync("https://data.hamburbur.org/").Result;
        using Stream        dataStream   = dataResponse.Content.ReadAsStreamAsync().Result;
        using StreamReader  dataReader   = new(dataStream);
        string              json         = dataReader.ReadToEnd().Trim();
        JObject             data         = JObject.Parse(json);

        JToken modVersionInfo =
                ((JArray)data["Mod Version Info"])!.FirstOrDefault(token => (string)token["Mod Name"] ==
                                                                            Constants.PluginName);

        if (modVersionInfo != null)
        {
            LatestVersion = new Version(((string)modVersionInfo["Latest Version"])!);
            Version minimumVersion = new(((string)modVersionInfo["Minimum Version"])!);

            NotLatestMessage = (string)modVersionInfo["Not Latest Message"];
            OutdatedMessage  = (string)modVersionInfo["Outdated Message"];

            Version localVersion = new(Constants.PluginVersion);

            if (localVersion < minimumVersion)
                VersionOutdated = true;
            else if (localVersion < LatestVersion)
                VersionNotLatest = true;
        }

        if (VersionOutdated)
            return;

        new GameObject($"{Constants.PluginName} Version Checking").AddComponent<ContinousVersionChecking>();
    }
}