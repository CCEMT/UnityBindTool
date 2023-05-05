#region Using

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace BindTool
{
    [Serializable]
    public class ComponentBindInfo
    {
        public string name;
        public TypeString[] typeStrings;
        public int index = 0;
        public GameObject instanceObject;
        public GameObject prefabObject;

        public AutoBindSetting autoBindSetting;

        public bool GameObjectEquals(object obj)
        {
            if (obj != null)
            {
                GameObject targetObject = obj as GameObject;
                if (targetObject != null)
                {
                    if (targetObject == instanceObject) { return true; }
                    else if (targetObject == prefabObject) return true;
                }
            }
            return false;
        }

        public string[] GetTypeStrings()
        {
            List<string> typeNames = new List<string>();
            int amount = typeStrings.Length;
            for (int i = 0; i < amount; i++) typeNames.Add(typeStrings[i].typeName);
            return typeNames.ToArray();
        }

        public GameObject GetObject()
        {
            if (prefabObject != null) return prefabObject;
            if (instanceObject != null) return instanceObject;
            return null;
        }

        public Object GetValue()
        {
            Type type = typeStrings[index].ToType();
            Type gameObjecType = typeof(GameObject);
            if (type == gameObjecType) return GetObject();

            Component component = null;
            if (this.prefabObject != null) { component = prefabObject.GetComponent(type); }
            if (component == null) { component = instanceObject.GetComponent(type); }

            return component;
        }

        public TypeString GetTypeString()
        {
            return typeStrings[index];
        }

        public bool AgainGet()
        {
            if (prefabObject == null) { prefabObject = CommonTools.GetPrefabAsset(GetObject()); }
            TypeString currenTypeString = typeStrings[index];
            AddComponentsTypes(GetObject());

            int amount = typeStrings.Length;
            index = -1;
            for (int i = 0; i < amount; i++)
            {
                if (typeStrings[i].Equals(currenTypeString))
                {
                    index = i;
                    return true;
                }
            }
            index = 0;
            return false;
        }

        public string GetTypeName()
        {
            return typeStrings[index].typeName;
        }

        public int SetIndex(TypeString typeString)
        {
            int amount = typeStrings.Length;
            for (int i = 0; i < amount; i++)
            {
                if (typeStrings[i].ToType() == typeString.ToType())
                {
                    index = i;
                    return i;
                }
            }
            return -1;
        }

        public ComponentBindInfo(Object ob)
        {
            if (ob != null)
            {
                if (ob is GameObject go)
                {
                    prefabObject = CommonTools.GetPrefabAsset(go);
                    instanceObject = go;
                    AddComponentsTypes(go);
                }
                else if (ob is Component co)
                {
                    prefabObject = CommonTools.GetPrefabAsset(co.gameObject);
                    instanceObject = co.gameObject;
                    AddComponentsTypes(co.gameObject);
                }
                else
                {
                    Type t = ob.GetType();
                    typeStrings = new TypeString[1] {new TypeString(t)};
                    index = 0;
                }
            }
        }

        private void AddComponentsTypes(GameObject go)
        {
            List<TypeString> typeStringList = new List<TypeString>();

            Type gameObjectType = typeof(GameObject);
            TypeString gameObjectTypeString = new TypeString(gameObjectType);
            typeStringList.Add(gameObjectTypeString);

            Component[] cs = go.GetComponents(typeof(Component));
            foreach (Component t in cs)
            {
                if (t == null) continue;
                Type type = t.GetType();
                TypeString typeString = new TypeString(type);
                typeStringList.Add(typeString);
            }

            typeStrings = typeStringList.ToArray();
        }
    }
}