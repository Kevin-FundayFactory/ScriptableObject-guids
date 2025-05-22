using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectGuids
{
    public class GuidObjectPostProcessor : AssetPostprocessor
    {
        private static Regex folderRegex = new Regex("^[a-zA-Z0-9\\/]+(?:( {1}\\d+))+$", RegexOptions.Compiled);
        private static Regex isNestedFolderRegex = new Regex("[a-zA-Z0-9.]+( \\d*)\\/", RegexOptions.Compiled);

        private static DateTime lastShownWarningTime = new DateTime(2000, 1, 1);

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so && HasGuidObjectMember(so, out string fieldName))
                {
                    var guidObj = GetGuidObject(so, fieldName);
                    if (string.IsNullOrWhiteSpace(guidObj?.GuidString) ||
                        IsGuidDuplicateInSameFolder(so, guidObj, path) ||
                        IsGuidDuplicateInParentFolder(so, guidObj, path) ||
                        guidObj.GuidString.Equals(Guid.Empty.ToString()))
                    {
                        guidObj.RefreshGuid();
                        EditorUtility.SetDirty(so);
                    }
                    else if ((DateTime.Now - lastShownWarningTime).TotalMinutes > 2 && isNestedFolderRegex.Match(GetFolderPath(so, path)).Groups.Count > 1)
                    {
                        lastShownWarningTime = DateTime.Now;
                        EditorUtility.DisplayDialog("STOP RIGHT THERE", "It appears that you MIGHT be trying to duplicate a nested folder structure of scriptable objects containing GuidObjects.\n\nBe very careful! The IDs of the newly created objects have NOT been updated and it WILL cause an issue with duplicated IDs!\n\nYou now have a few options.\n1: You can go ahead and delete the folder you just duplicated, then go into it and make duplicates on a lower level.\n\nHave fun!", "Thanks");
                        break;
                    }
                }
            }
        }

        private static GuidObject GetGuidObject(ScriptableObject so, string fieldName)
        {
            FieldInfo guidField = so.GetType().GetField(fieldName);
            return guidField.GetValue(so) as GuidObject;
        }

        private static bool HasGuidObjectMember(ScriptableObject so, out string fieldName)
        {
            var type = so.GetType();
            fieldName = "";
            FieldInfo[] fields = type.GetFields(
            BindingFlags.Public |
            BindingFlags.NonPublic | BindingFlags.Static |
            BindingFlags.Instance);

            foreach (FieldInfo info in fields)
            {
                if (info.FieldType == typeof(GuidObject))
                {
                    fieldName = info.Name;
                    return true;
                }
            }
            return false;
        }

        private static bool IsGuidDuplicateInSameFolder(ScriptableObject so, GuidObject guidObj, string path)
        {
            var folderPath = GetFolderPath(so, path);
            var siblingAssets = AssetDatabase.FindAssets("t: ScriptableObject", new string[] { folderPath });
            for (int i = 0; i < siblingAssets.Length; i++)
            {
                var sibling = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(siblingAssets[i]));
                if (sibling == so)
                    continue;

                if (HasGuidObjectMember(sibling, out var fieldName))
                {
                    var siblingGuid = GetGuidObject(sibling, fieldName);
                    if (siblingGuid.GuidString.Equals(guidObj.GuidString))
                        return true;
                }
            }
            return false;
        }

        private static bool IsGuidDuplicateInParentFolder(ScriptableObject so, GuidObject guidObj, string path)
        {
            var folderPath = GetFolderPath(so, path);
            var regexMatches = folderRegex.Match(folderPath);
            if (regexMatches.Success)
            {
                var siblingAssetPath = folderPath.Replace(regexMatches.Groups[1].Value, "");
                siblingAssetPath = Path.Combine(siblingAssetPath, Path.GetFileName(path));

                var absSiblingAssetPath = Path.Combine(Application.dataPath, siblingAssetPath.Substring(7));
                if (File.Exists(absSiblingAssetPath))
                {
                    var sibling = AssetDatabase.LoadAssetAtPath<ScriptableObject>(siblingAssetPath);
                    if (HasGuidObjectMember(sibling, out var fieldName))
                    {
                        var siblingGuid = GetGuidObject(sibling, fieldName);
                        if (siblingGuid.GuidString.Equals(guidObj.GuidString))
                            return true;
                    }
                }
            }

            return false;
        }

        private static string GetFolderPath(ScriptableObject so, string path)
        {
            return path.Replace($"/{so.name}.asset", "");
        }
    }
}