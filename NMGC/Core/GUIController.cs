using System;
using System.Linq;
using System.Reflection;
using NMGC.Version_Checking;
using NMGC.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NMGC.Core;

// pronounced /ˈɡuː.i/
public class GUIController : MonoBehaviour
{
    public static Transform MainPanel;

    private void Start()
    {
        MainPanel = transform.GetChild(0);

        if (VersionCheckingInitializer.VersionOutdated)
        {
            MainPanel.GetChild(2).gameObject.SetActive(false);
            MainPanel.GetChild(3).gameObject.SetActive(true);
            MainPanel.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                    $"<color=green>Latest Version: {VersionCheckingInitializer.LatestVersion}</color>\n<color=red>Installed Version: {Constants.PluginVersion}</color>";

            MainPanel.GetChild(3).GetChild(2).GetComponent<TextMeshProUGUI>().text =
                    VersionCheckingInitializer.OutdatedMessage;

            return;
        }

        if (VersionCheckingInitializer.VersionNotLatest)
            MainPanel.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text =
                    $"You Are Not On The Latest Version\n<size=50%>{VersionCheckingInitializer.NotLatestMessage}</size>";

        VersionCheckingInitializer.VersionOutdatedDetected += () =>
                                                              {
                                                                  MainPanel.GetChild(2).gameObject.SetActive(false);
                                                                  MainPanel.GetChild(3).gameObject.SetActive(true);
                                                                  MainPanel.GetChild(3).GetChild(1)
                                                                           .GetComponent<TextMeshProUGUI>().text =
                                                                          $"<color=green>Latest Version: {VersionCheckingInitializer.LatestVersion}</color>\n<color=red>Installed Version: {Constants.PluginVersion}</color>";

                                                                  MainPanel.GetChild(3).GetChild(2)
                                                                           .GetComponent<TextMeshProUGUI>().text =
                                                                          VersionCheckingInitializer.OutdatedMessage;

                                                                  MainPanel.GetChild(0)
                                                                           .GetComponentInChildren<TextMeshProUGUI>()
                                                                           .text =
                                                                          "No More Getting Checked\n<size=50%>A Mod To Keep You From Getting Accused</size>";

                                                                  Destroy(this);
                                                              };

        VersionCheckingInitializer.VersionNotLatestDetected +=
                () => MainPanel.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text =
                              $"You Are Not On The Latest Version\n<size=50%>{VersionCheckingInitializer.NotLatestMessage}</size>";

        Type[] nmgcViews = Assembly.GetExecutingAssembly().GetTypes()
                                   .Where(t => t.IsClass                                 && !t.IsAbstract &&
                                               typeof(MonoBehaviour).IsAssignableFrom(t) &&
                                               t.GetCustomAttribute<NMGCViewAttribute>() != null).ToArray();

        foreach (Type nmgcView in nmgcViews)
        {
            string viewName = nmgcView.GetCustomAttribute<NMGCViewAttribute>().ViewName;
            MainPanel.GetChild(1).GetChild(0).GetChild(0).GetChild(0).Find(viewName).GetComponent<Button>().onClick
                     .AddListener(() => ChangeCurrentView(viewName));

            MainPanel.GetChild(2).Find(viewName + "View").gameObject.AddComponent(nmgcView);
        }

        ChangeCurrentView("CustomProperties");

        MainPanel.GetChild(2).gameObject.SetActive(true);
        MainPanel.GetChild(3).gameObject.SetActive(false);

        MainPanel.gameObject.SetActive(false);
    }

    private void ChangeCurrentView(string newViewName)
    {
        foreach (Transform view in MainPanel.GetChild(2))
            view.gameObject.SetActive(view.name == newViewName + "View");
    }
}