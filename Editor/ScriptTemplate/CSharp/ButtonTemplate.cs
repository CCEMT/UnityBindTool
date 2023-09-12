using UnityBindTool;
using UnityEngine;
using UnityEngine.UI;

[TemplateClass(CSharpTemplateType.Type)]
public class ButtonTemplate : MonoBehaviour
{
    [TemplateField]
    public Button TemplateValue;

    [GenerateSingle]
    public void AddButtonAllClick()
    {
        this.TemplateValue.onClick.AddListener(Click);
    }

    public void Click() { }
}