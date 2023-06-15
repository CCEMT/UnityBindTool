namespace UnityBindTool
{
    [AssignTemplate(typeof(GeneratePathAttribute))]
    public class GeneratePathAttributeDispose : BaseCommonAttributeDispose
    {
        public override void DisposeField()
        {
            base.DisposeField();
        }
    }
}