using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NMGC.Views;

[NMGCView("FPS")]
public class FPSHandler : MonoBehaviour
{
    private static bool         spoofing;
    private static SpoofingType spoofingType = SpoofingType.Legit90;

    private readonly List<TextMeshProUGUI> buttonTexts = [];

    private void Start()
    {
        transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                                                                         {
                                                                             spoofing = !spoofing;
                                                                             transform.GetChild(0)
                                                                                            .GetComponentInChildren<
                                                                                                     TextMeshProUGUI>()
                                                                                            .text =
                                                                                     $"{(spoofing ? "Stop" : "Start")} Spoofing";
                                                                         });

        foreach (Transform child in transform.GetChild(1).GetChild(0).GetChild(0))
        {
            string buttonName = child.name;

            Button button = child.GetComponent<Button>();
            button.onClick.AddListener(() =>
                                               SetSpoofingType(GetSpoofingTypeFromString(buttonName))
            );

            TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
            buttonTexts.Add(text);
        }
    }

    private SpoofingType GetSpoofingTypeFromString(string input)
    {
        Dictionary<string, SpoofingType> allSpoofingTypes =
                Enum.GetValues(typeof(SpoofingType))
                    .Cast<SpoofingType>()
                    .ToDictionary(
                             spoofingType => spoofingType.ToString().Replace("Number", ""),
                             spoofingType => spoofingType
                     );

        return allSpoofingTypes[input];
    }

    private void SetSpoofingType(SpoofingType type)
    {
        spoofingType = type;
        string sanitizedEnumName = spoofingType.ToString().Replace("Number", "");
        foreach (TextMeshProUGUI buttonText in buttonTexts)
            buttonText.text = buttonText.transform.parent.name == sanitizedEnumName
                                      ? buttonText.text.Replace("<color=red><size=50%>Disabled</size></color>",
                                              "<color=green><size=50%>Enabled</size></color>")
                                      : buttonText.text.Replace("<color=green><size=50%>Enabled</size></color>",
                                              "<color=red><size=50%>Disabled</size></color>");
    }

    private enum SpoofingType
    {
        Legit60,
        Legit72,
        Legit90,
        Legit120,
        Legit144,
        Number67,
        Number69,
        Number61,
        Number41,
        Number21,
        Number255,
        Number0,
        Random0To255,
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.PackCompetitiveData))]
    private static class FPSPatch
    {
        private static bool Prefix(ref short __result)
        {
            if (!spoofing)
                return true;

            switch (spoofingType)
            {
                case SpoofingType.Legit60:
                    short[] numbersToSpoof60 = [57, 58, 58, 59, 59, 60, 60, 60, 61,];
                    __result = numbersToSpoof60[Random.Range(0, numbersToSpoof60.Length)];

                    break;

                case SpoofingType.Legit72:
                    short[] numbersToSpoof72 = [69, 70, 70, 71, 71, 72, 72, 72, 73,];
                    __result = numbersToSpoof72[Random.Range(0, numbersToSpoof72.Length)];

                    break;

                case SpoofingType.Legit90:
                    short[] numbersToSpoof90 = [87, 88, 88, 89, 89, 90, 90, 90, 91,];
                    __result = numbersToSpoof90[Random.Range(0, numbersToSpoof90.Length)];

                    break;

                case SpoofingType.Legit120:
                    short[] numbersToSpoof120 = [117, 118, 118, 119, 119, 120, 120, 120, 121,];
                    __result = numbersToSpoof120[Random.Range(0, numbersToSpoof120.Length)];

                    break;

                case SpoofingType.Legit144:
                    short[] numbersToSpoof144 = [141, 142, 142, 143, 143, 144, 144, 144, 145,];
                    __result = numbersToSpoof144[Random.Range(0, numbersToSpoof144.Length)];

                    break;

                case SpoofingType.Number67:
                    __result = 67;

                    break;

                case SpoofingType.Number69:
                    __result = 69;

                    break;

                case SpoofingType.Number61:
                    __result = 61;

                    break;

                case SpoofingType.Number41:
                    __result = 41;

                    break;

                case SpoofingType.Number21:
                    __result = 21;

                    break;

                case SpoofingType.Number255:
                    __result = 255;

                    break;

                case SpoofingType.Number0:
                    __result = 0;

                    break;

                case SpoofingType.Random0To255:
                    __result = (short)Random.Range(0, 255);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }
    }
}