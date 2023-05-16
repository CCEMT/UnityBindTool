using UnityEngine;
using UnityEngine.UI;

namespace BindTool.Template
{
    public class ButtonTemplate : MonoBehaviour
    {
        [TemplateField]
        public Button templateValue;

        [GenerateSingle(true)]
        public void AddButtonAllClick()
        {
            this.templateValue.onClick.AddListener(Click);
        }

        public void Click()
        {
            
        }
    }
}