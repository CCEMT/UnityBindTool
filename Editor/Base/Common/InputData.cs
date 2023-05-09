using System;
using Sirenix.OdinInspector;

[Serializable]
public class InputData
{
    private Action<string> inputCallback;

    [LabelText("输入：")]
    public string inputString;

    public InputData(Action<string> callback)
    {
        this.inputCallback = callback;
    }

    [Button("输入确认")]
    public void InputConfirm()
    {
        inputCallback?.Invoke(inputString);
    }
}