#region Using

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace BindTool
{
    public class BindComponents : MonoBehaviour
    {
        public Object bindRoot;
        public List<Object> bindComponentList = new List<Object>();
    }
}