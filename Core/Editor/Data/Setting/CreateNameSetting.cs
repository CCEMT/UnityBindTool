using System.Collections.Generic;
using UnityEngine;

namespace BindTool
{
    //[CreateAssetMenu(fileName = "CreateNameSetting", menuName = "CreatCreateNameSetting", order = 0)]
    public class CreateNameSetting : ScriptableObject
    {
        public string programName;
        public bool isBindAutoGenerateName;
        public List<NameReplaceData> nameReplaceDataList;

        #region Equals

        public override bool Equals(object other)
        {
            CreateNameSetting equalsValue = (CreateNameSetting) other;
            if (equalsValue != null) return Equals(equalsValue);
            return false;
        }

        protected bool Equals(CreateNameSetting other)
        {
            return base.Equals(other) && programName == other.programName && isBindAutoGenerateName == other.isBindAutoGenerateName && Equals(nameReplaceDataList, other.nameReplaceDataList);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (programName != null ? programName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ isBindAutoGenerateName.GetHashCode();
                hashCode = (hashCode * 397) ^ (nameReplaceDataList != null ? nameReplaceDataList.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}