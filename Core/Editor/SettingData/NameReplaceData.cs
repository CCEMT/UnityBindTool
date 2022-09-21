using System;

namespace BindTool
{
    [Serializable]
    public class NameReplaceData
    {
        public string targetName;
        public NameCheck nameCheck;

        #region Equals

        public override bool Equals(object obj)
        {
            NameReplaceData nameReplaceData = (NameReplaceData) obj;
            if (nameReplaceData != null) return Equals(nameReplaceData);
            return false;
        }

        protected bool Equals(NameReplaceData other)
        {
            return targetName == other.targetName && Equals(nameCheck, other.nameCheck);
        }

        public override int GetHashCode()
        {
            unchecked { return ((targetName != null ? targetName.GetHashCode() : 0) * 397) ^ (nameCheck != null ? nameCheck.GetHashCode() : 0); }
        }

        #endregion
    }
}