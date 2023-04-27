#region Using

using System;
using UnityEngine;

#endregion

namespace BindTool
{
    [Serializable]
    public class GenerateData
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
        public TypeString mergeTypeString;
    }
}