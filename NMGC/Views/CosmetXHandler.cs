using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NMGC.Views;

[NMGCView("CosmetX")]
public class CosmetXHandler : MonoBehaviour
{
    private static bool hiding;

    private void Start() => transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
        {
            hiding = !hiding;
            transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{(hiding ? "Unhide" : "Hide")} CosmetX";
        });
    
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.RequestCosmetics))]
    private static class CosmetXHidingPatch
    {
        private static bool Prefix(PhotonMessageInfoWrapped info, VRRig __instance) => !hiding;
    }
}