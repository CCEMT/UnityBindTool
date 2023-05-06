using UnityEngine;

namespace BindTool
{
    [System.Serializable]
    public class DataCollectionBindInfo
    {
        public string name;
        public CollectionType collectionType;
        public TypeString typeString;
        public Object[] bindObjects;
    }
}