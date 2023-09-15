namespace UnityBindTool
{
    [AssignTemplate(typeof(GenerateSingleAttribute))]
    public class GenerateSingleAttributeDispose : BaseTypeAttributeDispose
    {
        public override void Dispose()
        {
            TypeDisposeCotentData typeDisposeCotentData = this.disposeCotentData as TypeDisposeCotentData;

            typeDisposeCotentData.memberDeclarationSyntaxs.Clear();
            typeDisposeCotentData.memberDeclarationSyntaxs.Add(typeDisposeCotentData.originalContent);

            GenerateSingleAttribute generateSingleAttribute = attributeValue as GenerateSingleAttribute;
            if (generateSingleAttribute.isSingleContent == false) { }
        }
    }
}