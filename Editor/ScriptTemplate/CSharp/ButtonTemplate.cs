using UnityEngine;
using UnityEngine.UI;

namespace BindTool.Template
{
    public class ButtonTemplate : MonoBehaviour,ITemplate<Button>
    {
        public Button TemplateValue { get; }

        [GenerateSingle(true)]
        public void AddButtonAllClick()
        {
            this.TemplateValue.onClick.AddListener(Click);
        }

        public void Click() { }
    }
}