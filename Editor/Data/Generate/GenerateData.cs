#region Using

using System;
using Sirenix.Serialization;
using UnityEngine;

#endregion

namespace BindTool
{
    [Serializable]
    public class GenerateData : ISerializationCallbackReceiver
    {
        [SerializeField]
        private byte[] objectInfoBytes;

        //新脚本的名称
        public string newScriptName;

        //绑定的脚本类型
        public TypeString mergeTypeString;

        //绑定的物体
        public GameObject bindObject;

        //绑定信息
        [NonSerialized, OdinSerialize]
        public ObjectInfo objectInfo;

        //获取绑定数据的函数名称
        public string getBindDataMethodName;

        public void OnBeforeSerialize()
        {
            objectInfoBytes = SerializationUtility.SerializeValue(objectInfo, DataFormat.Binary);
        }

        public void OnAfterDeserialize()
        {
            objectInfo = SerializationUtility.DeserializeValue<ObjectInfo>(objectInfoBytes, DataFormat.Binary);
        }
    }
}