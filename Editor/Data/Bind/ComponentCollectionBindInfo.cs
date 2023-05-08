using System;
using System.Linq;

namespace BindTool
{
    [Serializable]
    public class ComponentCollectionBindInfo
    {
        public string name;
        public CollectionType collectionType;
        public ComponentBindInfo[] componentBindInfos;

        public int index;
        public TypeString[] typeStrings;

        public string[] GetTypeStrings()
        {
            return typeStrings.Select((v) => v.typeName).ToArray();
        }

        public TypeString GetTypeString()
        {
            return typeStrings[index];
        }
    }
}