using System;
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
        private const string GitUrl = "https://github.com/Armangi1312/AMBehaviorSystem.git";
        private const string SessionKey = "AM.UpdateChecker.Checked";

        private static RemoveRequest removeRequest;
        private static AddRequest addRequest;

        static UpdateChecker()
        {
            if(SessionState.GetBool(SessionKey, false))
                return;
            SessionState.SetBool(SessionKey, true);

            EditorApplication.delayCall += OnEditorReady;
        }

        private static void OnEditorReady()
        {
            EditorApplication.delayCall -= OnEditorReady;

            ShowUpdateDialog();
        }

        private static void ShowUpdateDialog()
        {
            bool update = EditorUtility.DisplayDialog(
                "Update",
                "unity-am-modular-behavior-system has been archived. " +
                "A new version, featuring a pipeline system and source code auto-generation capabilities, continues under the name AMBehaviorSystem. " +
                "Would you like to migrate to AMBehaviorSystem?",
                "Yes",
                "No"
            );

            if(update)
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
            if(!removeRequest.IsCompleted)
                return;

            EditorApplication.update -= WaitForRemove;

            if(removeRequest.Status != StatusCode.Success)
            {
                Debug.LogError($"[UpdateChecker] Failed to remove package: {removeRequest.Error.message}");
                return;
            }

            Debug.Log("[UpdateChecker] Package removed. Installing latest version...");
            addRequest = Client.Add(GitUrl);
            EditorApplication.update += WaitForAdd;
        }

        private static void WaitForAdd()
        {
            if(!addRequest.IsCompleted)
                return;

            EditorApplication.update -= WaitForAdd;

            if(addRequest.Status != StatusCode.Success)
            {
                Debug.LogError($"[UpdateChecker] Failed to install package: {addRequest.Error.message}");
                return;
            }

            Debug.Log("[UpdateChecker] Update complete!");
            EditorUtility.DisplayDialog(
                "Update Complete",
                "The package has been updated to the latest version.",
                "OK"
            );
        }
    }
}