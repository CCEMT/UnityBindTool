///该脚本为模板方法
///注意：
///需要生成的方法写入#region Template Method
///生成方法使用到的类型必须为[命名空间].[类型名]，如果使用using引用命名空间生成时可能会生成失败
namespace BindTool.Template
{
    [BindTool.ScriptTemplate]
	public class ButtonTemplate : UnityEngine.MonoBehaviour
	{
		public UnityEngine.UI.Button templateValue;
        #region Template Method
        public void AddClick(UnityEngine.Events.UnityAction callBack)
        {
            templateValue.onClick.AddListener(callBack);
        }

        public void RemoveClick(UnityEngine.Events.UnityAction callBack)
        {
            templateValue.onClick.RemoveListener(callBack);
        }

        public void RemoveAllClick()
        {
            templateValue.onClick.RemoveAllListeners();
        }
        #endregion
    }
}
