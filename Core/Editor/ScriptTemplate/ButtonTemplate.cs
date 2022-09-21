using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BindTool.Template
{
    public class ButtonTemplate : MonoBehaviour
    {
        public Button templateValue;

        #region Template Method

        public void AddClick(UnityAction action)
        {
            templateValue.onClick.AddListener(action);
        }

        #endregion
    }
}