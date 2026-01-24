using System.Collections;
using System.Collections.Generic;
using Console;
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
    private static          bool                       hasInitCustomProps;
    private static readonly List<string>               appliedCustomProperties = [];
    private                 Dictionary<string, string> knownCustomProperties   = [];

    private void Start()
    {
        knownCustomProperties = DataHamburburOrg.Data["Known Mods"]!.ToObject<Dictionary<string, string>>();
        knownCustomProperties.AddAll(DataHamburburOrg.Data["Known Cheats"]!.ToObject<Dictionary<string, string>>());

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
            button.name                                           = propertyName;
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
                transform.GetChild(0).GetChild(0).GetChild(0).Find(key)
                         .SetParent(transform.GetChild(1).GetChild(0).GetChild(0));
            }
            else
            {
                GameObject button = Instantiate(exampleButton, transform.GetChild(1).GetChild(0).GetChild(0));
                button.GetComponent<Button>().onClick.AddListener(() => SetCustomProperty(key, button));
                button.name                                           = key;
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

        button.transform.SetParent(adding
                                           ? transform.GetChild(1).GetChild(0).GetChild(0)
                                           : transform.GetChild(0).GetChild(0).GetChild(0));

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { propertyName, adding ? true : null }, });

        if (!adding)
            appliedCustomProperties.Remove(propertyName);
    }

    [HarmonyPatch]
    private static class CustomPropertiesPatches
    {
        /*
Patches taken from https://codeberg.org/gizmogoat/StopUsingUselessCustomPropertiesInYourTerribleModsTheyArentNeededYouDontNeedToNetworkUselessValues

MIT License

Copyright (c) 2025 gizmogoat

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
         */

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