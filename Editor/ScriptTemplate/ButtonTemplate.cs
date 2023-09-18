using UnityEngine;
using UnityEngine.UI;

namespace UnityBindTool.Template
{
    [TemplateClass(CSharpTemplateType.Type)]
    public class ButtonTemplate : MonoBehaviour
    {
        [TemplateField]
        public Button TemplateValue;

        public void Click() { }
    }
}