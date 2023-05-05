using System.Collections.Generic;
using UnityEngine;

namespace BindTool
{
    //[CreateAssetMenu(fileName = "CreateNameSetting", menuName = "CreatCreateNameSetting", order = 0)]
    public class CreateNameSetting : ScriptableObject
    {
        public string programName;
        public bool isBindAutoGenerateName;
        public List<NameReplaceData> variableNameReplaceDataList;
        public List<NameReplaceData> propertyNameReplaceDataList;
    }
}