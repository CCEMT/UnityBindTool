#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace BindTool
{
    public class BindComponents : MonoBehaviour
    {
#if UNITY_EDITOR
        [HideInInspector]
        public Type targetType;

        public List<string> bindName = new List<string>();
#endif

        [ReadOnly]
        public List<Object> bindComponentList = new List<Object>();

        [ReadOnly, NonSerialized, OdinSerialize]
        public List<IEnumerator> bindCollectionList = new List<IEnumerator>();
    }
}