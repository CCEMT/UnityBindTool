using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace UnityBindTool
{
    [Serializable]
    public class CSharpNameGenerateSetting
    {
        [LabelText("属性名称替换")]
        public List<NameReplaceData> propertyNameReplaceDataList = new List<NameReplaceData>();

        [LabelText("方法名称替换")]
        public List<NameReplaceData> methodNameReplaceDataList = new List<NameReplaceData>();
    }
}