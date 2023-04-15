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
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj is IGuidObject gu && obj is ScriptableObject so)
                {
                    if (string.IsNullOrEmpty(gu.GuidObject.GuidString) || IsGuidDuplicate(so, path))
                    {
                        gu.GuidObject.RefreshGuid();
                        dirtyCount++;
                        EditorUtility.SetDirty(obj);
                    }
                }
            }
            if (dirtyCount > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static bool IsGuidDuplicate(ScriptableObject so, string path)
        {
            var iGuid = so as IGuidObject;
            var folderPath = path.Replace("/" + so.name + ".asset", "");
            var siblingAssets = AssetDatabase.FindAssets("t: ScriptableObject", new string[] { folderPath });
            for (int i = 0; i < siblingAssets.Length; i++)
            {
                var sibling = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(siblingAssets[i]));
                if (sibling == so)
                    continue;

                if (sibling is IGuidObject gu && gu.GuidObject.GuidString.Equals(iGuid.GuidObject.GuidString))
                    return true;
            }
            return false;
        }
    }

}