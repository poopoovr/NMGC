using System.Collections;
using System.Collections.Generic;
using GorillaExtensions;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace NMGC.Views;

[NMGCView("CustomProperties")]
public class CustomPropertiesHandler : MonoBehaviour
{
    private static bool hasInitCustomProps;
    private static readonly List<string> appliedCustomProperties = [];
    private Dictionary<string, string> knownCustomProperties = [];

    private static readonly Dictionary<string, string> KnownCustomProperties = new()
    {
        { "bark", "Bark" },
        { "monkeMapLoader", "MonkeMapLoader" },
        { "utilla", "Utilla" },
        { "computerInterface", "ComputerInterface" }
    };

    private void Start()
    {
        knownCustomProperties = new Dictionary<string, string>(KnownCustomProperties);
        SetupButtons();
    }

    private void SetupButtons()
    {
        GameObject exampleButton = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject;
        Hashtable currentCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        foreach ((string propertyName, string modName) in knownCustomProperties)
        {
            GameObject button = Instantiate(exampleButton, transform.GetChild(0).GetChild(0).GetChild(0));
            button.GetComponent<Button>().onClick.AddListener(() => SetCustomProperty(propertyName, button));
            button.name = propertyName;
            button.GetComponentInChildren<TextMeshProUGUI>().text = $"{propertyName}\n({modName})";
        }

        Hashtable propertiesToRemove = [];

        foreach (DictionaryEntry property in currentCustomProperties)
        {
            if (property.Key is not string key)
            {
                propertiesToRemove.Add(property.Key, null);
                continue;
            }

            if (key == "didTutorial")
                continue;

            appliedCustomProperties.Add(key);
            if (knownCustomProperties.ContainsKey(key))
            {
                transform.GetChild(0).GetChild(0).GetChild(0).Find(key).SetParent(transform.GetChild(1).GetChild(0).GetChild(0));
            }
            else
            {
                GameObject button = Instantiate(exampleButton, transform.GetChild(1).GetChild(0).GetChild(0));
                button.GetComponent<Button>().onClick.AddListener(() => SetCustomProperty(key, button));
                button.name = key;
                button.GetComponentInChildren<TextMeshProUGUI>().text = key;
            }
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToRemove);
        Destroy(exampleButton);
        hasInitCustomProps = true;
    }

    private void SetCustomProperty(string propertyName, GameObject button)
    {
        bool adding = !appliedCustomProperties.Contains(propertyName);
        if (adding)
            appliedCustomProperties.Add(propertyName);

        button.transform.SetParent(adding ? transform.GetChild(1).GetChild(0).GetChild(0) : transform.GetChild(0).GetChild(0).GetChild(0));
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { propertyName, adding ? true : null }, });

        if (!adding)
            appliedCustomProperties.Remove(propertyName);
    }

    [HarmonyPatch]
    private static class CustomPropertiesPatches
    {
        [HarmonyPatch(typeof(Player), nameof(Player.CustomProperties), MethodType.Setter)]
        [HarmonyPrefix]
        private static bool PatchOne(ref Hashtable value)
        {
            if (!hasInitCustomProps)
                return true;

            foreach (DictionaryEntry prop in value)
                if ((string)prop.Key != "didTutorial" && !appliedCustomProperties.Contains((string)prop.Key))
                    return false;

            return true;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetCustomProperties), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool PatchTwo(ref Hashtable propertiesToSet)
        {
            if (!hasInitCustomProps)
                return true;

            foreach (DictionaryEntry prop in propertiesToSet)
                if ((string)prop.Key != "didTutorial" && !appliedCustomProperties.Contains((string)prop.Key))
                    return false;

            return true;
        }
    }
}
