using UnityEngine;

namespace BindTool
{
    [System.Serializable]
    public class DataCollectionBindInfo
    {
        public string name;
        public CollectionType collectionType;
        public TypeString targetType;
        public Object[] bindObjects;
    }
}