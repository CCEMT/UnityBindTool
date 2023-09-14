using System;
using System.Linq;
using Sirenix.Utilities;

namespace UnityBindTool
{
    public class TemplateDisposeHelper
    {
        public static void Dispose<T>(BaseTemplateDispose templateDispose, DisposeCotentData disposeCotentData, Attribute[] attributes) where T : BaseAttributeDispose
        {
            int amount = attributes.Length;
            for (int i = 0; i < amount; i++)
            {
                Attribute attribute = attributes[i];
                Type attributeType = attribute.GetType();

                Type firstOrDefault = GetDisposeType<T>(attributeType);
                if (firstOrDefault == null) continue;
                T attributeDispose = Activator.CreateInstance(firstOrDefault) as T;
                attributeDispose.templateDispose = templateDispose;
                attributeDispose.attributeValue = attribute;

                attributeDispose.disposeCotentData = disposeCotentData;

                attributeDispose.Dispose();
            }
        }

        static Type GetDisposeType<T>(Type attributeType) where T : BaseAttributeDispose
        {
            Type baseAttributeDisposeType = typeof(T);
            Type[] types = baseAttributeDisposeType.Assembly.GetTypes();
            Type[] baseTypes = types.Where((type) => type.IsSubclassOf(baseAttributeDisposeType)).ToArray();
            Type firstOrDefault = baseTypes.FirstOrDefault(type => {
                AssignTemplate assignTemplate = type.GetCustomAttribute<AssignTemplate>();
                if (assignTemplate == null) return false;
                return assignTemplate.type == attributeType;
            });
            return firstOrDefault;
        }
    }
}