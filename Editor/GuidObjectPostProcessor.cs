using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;
namespace ScriptableObjectGuids
{
    public class GuidObjectPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            int dirtyCount = 0;
            foreach (var path in importedAssets)
            {
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so && HasGuidObjectMember(so, out string fieldName))
                {
                    var guidObj = GetGuidObject(so, fieldName);
                    if (string.IsNullOrWhiteSpace(guidObj?.GuidString) || IsGuidDuplicate(so, guidObj, path) || guidObj.GuidString.Equals(Guid.Empty.ToString()))
                    {
                        guidObj.RefreshGuid();
                        dirtyCount++;
                        EditorUtility.SetDirty(so);
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

        private static bool IsGuidDuplicate(ScriptableObject so, GuidObject guidObj, string path)
        {
            var folderPath = path.Replace("/" + so.name + ".asset", "");
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
    }
}