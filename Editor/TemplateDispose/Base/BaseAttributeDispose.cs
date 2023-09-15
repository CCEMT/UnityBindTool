using System;

namespace UnityBindTool
{
    public abstract class BaseAttributeDispose
    {
        public BaseTemplateDispose templateDispose;
        public Attribute attributeValue;

        public DisposeCotentData disposeCotentData;

        public abstract void Dispose();
    }
}