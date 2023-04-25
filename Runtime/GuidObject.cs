using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectGuids
{
    [Serializable]
    public class GuidObject : ISerializationCallbackReceiver
    {
        [ReadOnly] //Useful to look at in the Editor
        public string GuidString;
        public Guid Guid;

        public void OnAfterDeserialize()
        {
            try
            {
                Guid = Guid.Parse(GuidString);
            }
            catch
            {
                Guid = Guid.Empty;
                Debug.LogWarning($"Attempted to parse invalid GUID string '{GuidString}'. GUID will set to System.Guid.Empty");
            }
        }

        public void OnBeforeSerialize()
        {
            GuidString = Guid.ToString();
        }

        public void RefreshGuid()
        {
            Guid = Guid.NewGuid();
            GuidString = Guid.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is GuidObject guid &&
                    this.Guid.Equals(guid.Guid);
        }

        public override int GetHashCode()
        {
            return -1324198676 + Guid.GetHashCode();
        }
    }
}