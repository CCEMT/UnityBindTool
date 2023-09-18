using System.IO;

namespace UnityBindTool
{
    public class LuaGenerator : IGenerator
    {
        private CompositionSetting selectSetting;
        private ScriptSetting scriptSetting;
        private LuaScriptSetting luaScriptSetting;

        private GenerateData generateData;

        private NameDisposeCentre nameDisposeCentre;

        public void Init(CompositionSetting setting, GenerateData generateData)
        {
            selectSetting = setting;
            scriptSetting = this.selectSetting.scriptSetting;
            luaScriptSetting = this.scriptSetting.luaScriptSetting;
            this.generateData = generateData;
            nameDisposeCentre = new NameDisposeCentre();
        }

        public void Write(string scriptPath)
        {
            string fullPath = $"{scriptPath}{generateData.newScriptName}{CommonConst.LuaFileSuffix}";
            if (File.Exists(fullPath)) File.Delete(fullPath);

            string templateContent = luaScriptSetting.luaTemplate.text;

            string baseClassContent = templateContent.Replace(CommonConst.LuaBaseClassNameIdentifier, luaScriptSetting.baseClassName);
            string inheritClassContent = baseClassContent.Replace(CommonConst.LuaInheritClassNameIdentifier, luaScriptSetting.inheritClass);

            string classNameContent = inheritClassContent.Replace(CommonConst.LuaClassNameIdentifier, generateData.newScriptName);

            string generateContent = GetGenerateContent();
            string content = classNameContent.Replace(CommonConst.LuaGenerateContentIdentifier, generateContent);

            StreamWriter mainWriter = File.CreateText(fullPath);
            mainWriter.Write(content);
            mainWriter.Close();
        }

        string GetGenerateContent()
        {
            string generateContent = "";
            IRepetitionNameDisposer fieldNameDisposer = RepetitionNameDisposeFactory.GetRepetitionNameDisposer(this.scriptSetting.nameSetting.repetitionNameDispose);

            int bindAmount = this.generateData.objectInfo.bindDataList.Count;
            for (int i = 0; i < bindAmount; i++)
            {
                BindData bindData = this.generateData.objectInfo.bindDataList[i];
                string fieldName = fieldNameDisposer.DisposeName(this.nameDisposeCentre, bindData.name);
                string bindContent = $"\tself.{fieldName} = targetObject[{i}]\n";
                generateContent += bindContent;
            }

            int collectionAmount = this.generateData.objectInfo.bindCollectionList.Count;
            for (int i = 0; i < collectionAmount; i++)
            {
                BindCollection collection = this.generateData.objectInfo.bindCollectionList[i];
                string fieldName = fieldNameDisposer.DisposeName(this.nameDisposeCentre, collection.name);
                string bindContent = $"\tself.{fieldName} = bindCollectionList[{i}]\n";
                generateContent += bindContent;
            }

            return generateContent;
        }
    }
}