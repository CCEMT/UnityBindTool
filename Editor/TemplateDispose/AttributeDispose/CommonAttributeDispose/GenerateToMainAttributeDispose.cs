namespace UnityBindTool
{
    [AssignTemplate(typeof(GenerateToMainAttribute))]
    public class GenerateToMainAttributeDispose : BaseCommonAttributeDispose
    {
        public override void Dispose()
        {
            disposeCotentData.generateTarget = templateDispose.mainTargetClass;
        }
    }
}