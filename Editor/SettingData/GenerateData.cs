#region Using

using UnityEngine;

#endregion

namespace BindTool
{
    public class GenerateData : ScriptableObject
    {
        //是否Build
        public bool isStartBuild;

        //新脚本的名称
        public string newScriptName;

        //绑定的物体
        public GameObject bindObject;
        //绑定信息
        public ObjectInfo objectInfo;

        //获取绑定数据的函数名称
        public string getBindDataMethodName;

        //绑定的脚本类型
        public TypeString addTypeString;
    }
}