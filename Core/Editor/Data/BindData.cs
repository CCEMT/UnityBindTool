using System.Collections.Generic;
using UnityEngine;

namespace BindTool
{
    //[CreateAssetMenu(fileName = "BindData", menuName = "CreatBindData", order = 0)]
    public class BindData : ScriptableObject
    {
        public List<ObjectInfo> objectInfoList;

        #region Equals

        public override bool Equals(object other)
        {
            BindData bindData = (BindData) other;
            if (bindData != null) { return Equals(bindData); }
            else { return false; }
        }

        protected bool Equals(BindData other)
        {
            return base.Equals(other) && Equals(objectInfoList, other.objectInfoList);
        }

        public override int GetHashCode()
        {
            unchecked { return (base.GetHashCode() * 397) ^ (objectInfoList != null ? objectInfoList.GetHashCode() : 0); }
        }

        #endregion
    }
}