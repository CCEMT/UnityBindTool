#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace BindTool
{
    public class BindComponents : SerializedMonoBehaviour
    {
#if UNITY_EDITOR
        [HideInInspector]
        public Type targetType;

        [HideInInspector]
        public List<string> bindName = new List<string>();

        [HideInInspector]
        public List<string> bindCollectionName = new List<string>();
#endif

        [ListDrawerSettings(IsReadOnly = true)]
        public List<Object> bindDataList = new List<Object>();

        [ListDrawerSettings(IsReadOnly = true)]
        public List<IEnumerable> bindCollectionList = new List<IEnumerable>();
    }
}