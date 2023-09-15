using UnityBindTool;
using UnityEngine;
using UnityEngine.UI;

[TemplateClass(CSharpTemplateType.Type)]
public class ButtonTemplate : MonoBehaviour
{
    [TemplateField]
    public Button TemplateValue;

    public void Click() { }
}