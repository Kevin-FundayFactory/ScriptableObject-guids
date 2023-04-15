using System;

namespace ScriptableObjectGuids
{
    [Serializable]
    public class GuidObject
    {
        [ReadOnly] //Useful to look at in the Editor
        public string GuidString;
        public Guid Guid;

        public void RefreshGuid()
        {
            Guid = Guid.NewGuid();
            GuidString = Guid.ToString();
        }
    }
}