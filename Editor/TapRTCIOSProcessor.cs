using System.IO;
using TapTap.Common.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace TapTap.RTC.Editor
{
    public class TapMomentIOSProcessor : MonoBehaviour
    {
        [PostProcessBuild(107)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget != BuildTarget.iOS) return;

            var projPath = TapCommonCompile.GetProjPath(path);
            var proj = TapCommonCompile.ParseProjPath(projPath);
            var target = TapCommonCompile.GetUnityTarget(proj);
            var unityFrameworkTarget = TapCommonCompile.GetUnityFrameworkTarget(proj);
            if (TapCommonCompile.CheckTarget(target))
            {
                Debug.LogError("Unity-iPhone is NUll");
                return;
            }
            proj.AddFileToBuild(unityFrameworkTarget,
                proj.AddFile("usr/lib/libresolv.9.tbd", "libresolv.9.tbd", PBXSourceTree.Sdk));
                
            File.WriteAllText(projPath, proj.WriteToString());
            
            Debug.Log("TapRTC add lib Success!");

        }
    }
}