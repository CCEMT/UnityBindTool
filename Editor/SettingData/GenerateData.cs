#region Using

using UnityEngine;

#endregion

namespace BindTool
{
    public class GenerateData : ScriptableObject
    {
        public bool isStartBuild;

        //新脚本的名称
        public string newScriptName;

        public GameObject bindObject;
        public ObjectInfo objectInfo;
        public string getBindDataMethodName;

        //绑定的脚本
        public TypeString addTypeString;
    }
}