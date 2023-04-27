#region Using

using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

#endregion

namespace BindTool
{
    public class BindComponents : MonoBehaviour
    {
        public Object bindRoot;
        [ReadOnly]
        public List<Object> bindComponentList = new List<Object>();
    }
}