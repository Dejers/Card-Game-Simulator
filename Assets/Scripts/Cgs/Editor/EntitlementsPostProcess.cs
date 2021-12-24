﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Cgs.Editor
{
    public class EntitlementsPostProcess : ScriptableObject
    {
        public DefaultAsset entitlementsFile;

        [PostProcessBuild]
        public static void OnPostProcess(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            var dummy = CreateInstance<EntitlementsPostProcess>();
            var file = dummy.entitlementsFile;
            DestroyImmediate(dummy);
            if (file == null)
            {
                Debug.LogError("EntitlementsPostProcess::entitlementsFileMissing!");
                return;
            }

            var pbxProjectPath = PBXProject.GetPBXProjectPath(buildPath);
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxProjectPath);

            const string targetName = "Unity-iPhone";
            var targetGuid = pbxProject.GetUnityMainTargetGuid();
            var src = AssetDatabase.GetAssetPath(file);
            var fileName = Path.GetFileName(src);
            var dst = buildPath + "/" + targetName + "/" + fileName;
            FileUtil.CopyFileOrDirectory(src, dst);
            pbxProject.AddFile(targetName + "/" + fileName, fileName);
            pbxProject.AddBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", targetName + "/" + fileName);
            pbxProject.WriteToFile(pbxProjectPath);
        }
    }
}