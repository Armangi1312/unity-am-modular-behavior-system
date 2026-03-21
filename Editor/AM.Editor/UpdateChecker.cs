using System;
using System.Net.Http;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace AM.Editor
{
    [InitializeOnLoad]
    public static class UpdateChecker
    {
        private const string PackageName = "com.ghoonykim.am.modular-behavior-system";
        private const string GitUrl = "https://github.com/Armangi1312/unity-am-modular-behavior-system.git";

        private const string RemoteVersionUrl =
            "https://raw.githubusercontent.com/Armangi1312/unity-am-modular-behavior-system/main/version.json";

        private const string LocalVersionPath =
            "Packages/com.ghoonykim.am.modular-behavior-system/version.json";

        private static RemoveRequest removeRequest;
        private static AddRequest addRequest;

        [Serializable]
        private class VersionInfo
        {
            public string version;
            public string updateImportance;
        }

        static UpdateChecker()
        {
            EditorApplication.delayCall += OnEditorReady;
        }

        private static async void OnEditorReady()
        {
            EditorApplication.delayCall -= OnEditorReady;

            try
            {
                // Read local version.json
                string localJson = System.IO.File.ReadAllText(LocalVersionPath);
                var localInfo = JsonUtility.FromJson<VersionInfo>(localJson);

                // Read remote version.json
                using var client = new HttpClient();
                string remoteJson = await client.GetStringAsync(RemoteVersionUrl);
                var remoteInfo = JsonUtility.FromJson<VersionInfo>(remoteJson);

                if (IsNewVersionAvailable(localInfo.version, remoteInfo.version))
                    ShowUpdateDialog(localInfo.version, remoteInfo.version, remoteInfo.updateImportance);
                else
                    Debug.Log("[UpdateChecker] Already up to date.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UpdateChecker] Failed to check version: {e.Message}");
            }
        }

        private static bool IsNewVersionAvailable(string current, string latest)
        {
            if (!Version.TryParse(current, out var currentVer)) return false;
            if (!Version.TryParse(latest, out var latestVer)) return false;
            return latestVer > currentVer;
        }

        private static void ShowUpdateDialog(string current, string latest, string importance)
        {
            string importanceLabel = importance switch
            {
                "hotfix" => "Hotfix",
                "major" => "Major Update",
                "minor" => "Minor Update",
                _ => importance
            };

            bool update = EditorUtility.DisplayDialog(
                "Update Available",
                $"[{importanceLabel}]\n\nA new version is available!\n\nCurrent: {current}\nLatest: {latest}\n\nWould you like to update now?",
                "Update",
                "Later"
            );

            if (update)
                StartUpdate();
        }

        private static void StartUpdate()
        {
            Debug.Log("[UpdateChecker] Removing package...");
            removeRequest = Client.Remove(PackageName);
            EditorApplication.update += WaitForRemove;
        }

        private static void WaitForRemove()
        {
            if (!removeRequest.IsCompleted) return;

            EditorApplication.update -= WaitForRemove;

            if (removeRequest.Status == StatusCode.Success)
            {
                Debug.Log("[UpdateChecker] Package removed. Installing latest version...");
                addRequest = Client.Add(GitUrl);
                EditorApplication.update += WaitForAdd;
            }
            else
            {
                Debug.LogError($"[UpdateChecker] Failed to remove package: {removeRequest.Error.message}");
            }
        }

        private static void WaitForAdd()
        {
            if (!addRequest.IsCompleted) return;

            EditorApplication.update -= WaitForAdd;

            if (addRequest.Status == StatusCode.Success)
            {
                Debug.Log("[UpdateChecker] Update complete!");
                EditorUtility.DisplayDialog(
                    "Update Complete",
                    "The package has been updated to the latest version.",
                    "OK"
                );
            }
            else
            {
                Debug.LogError($"[UpdateChecker] Failed to install package: {addRequest.Error.message}");
            }
        }
    }
}