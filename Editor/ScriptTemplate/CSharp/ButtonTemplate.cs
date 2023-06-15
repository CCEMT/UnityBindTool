using UnityEngine;
using UnityEngine.UI;

namespace BindTool.Template
{
    
    public class ButtonTemplate : MonoBehaviour
    {
        [TemplateField]
        public Button TemplateValue;

        [GenerateSingle]
        public void AddButtonAllClick()
        {
            this.TemplateValue.onClick.AddListener(Click);
        }

        public void Click()
        {
            
        }
    }
}