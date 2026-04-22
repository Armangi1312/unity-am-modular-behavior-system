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
        private const string RemotePackageUrl = "https://raw.githubusercontent.com/Armangi1312/unity-am-modular-behavior-system/main/package.json";
        private const string LocalPackagePath = "Packages/com.ghoonykim.am.modular-behavior-system/package.json";

        private static RemoveRequest removeRequest;
        private static AddRequest addRequest;

        private const string SessionKey = "AM.UpdateChecker.Checked";

        [Serializable]
        private class PackageInfo
        {
            public string version;
        }

        static UpdateChecker()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);

            EditorApplication.delayCall += OnEditorReady;
        }

        private static async void OnEditorReady()
        {
            EditorApplication.delayCall -= OnEditorReady;

            try
            {
                string localJson = System.IO.File.ReadAllText(LocalPackagePath);
                var localInfo = JsonUtility.FromJson<PackageInfo>(localJson);

                using var client = new HttpClient();
                string remoteJson = await client.GetStringAsync(RemotePackageUrl);
                var remoteInfo = JsonUtility.FromJson<PackageInfo>(remoteJson);

                if (IsNewVersionAvailable(localInfo.version, remoteInfo.version))
                    ShowUpdateDialog(localInfo.version, remoteInfo.version);
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

        private static void ShowUpdateDialog(string current, string latest)
        {
            string updateType = GetUpdateType(current, latest);
            string updateDescription = GetUpdateDescription(updateType);

            bool update = EditorUtility.DisplayDialog(
                $"Update Available [{updateType}]",
                $"A new version is available.\n\n" +
                $"Current: {current}\n" +
                $"Latest:  {latest}\n\n" +
                $"Update Type: {updateType}\n" +
                $"{updateDescription}\n\n" +
                $"Would you like to update now?",
                "Yes",
                "No"
            );

            if (update)
                StartUpdate();
        }

        private static string GetUpdateType(string current, string latest)
        {
            Version.TryParse(current, out var cur);
            Version.TryParse(latest, out var lat);

            if (lat.Major > cur.Major) return "Major";
            if (lat.Minor > cur.Minor) return "Minor";
            return "Fix";
        }
        
        private static string GetUpdateDescription(string updateType) => updateType switch
        {
            "Major" => "! Major update: Breaking changes may exist.\n   Review the changelog before updating.",
            "Minor" => "+ Minor update: New features added.\n   Generally safe to update.",
            _       => "* Fix update: Bug fixes only.\n   Safe to update.",
        };

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
