using UnityEngine;

namespace UnityBindTool.Template
{
    [TemplateClass(CSharpTemplateType.Common)]
    public class MainTemplate : MonoBehaviour
    {
        [GeneratePath(GeneratePathAttribute.PathType.Prefab)]
        public string PrefabPath;
    }
}