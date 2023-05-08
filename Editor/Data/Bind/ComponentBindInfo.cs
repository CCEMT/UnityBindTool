#region Using

using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool GameObjectEquals(object obj)
        {
            if (obj == null) return false;
            GameObject targetObject = obj as GameObject;
            if (targetObject == null) return false;
            if (targetObject == this.instanceObject) { return true; }
            else if (targetObject == this.prefabObject) return true;
            return false;
        }

        public string[] GetTypeStrings()
        {
            return typeStrings.Select((v) => v.typeName).ToArray();
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
                if (! this.typeStrings[i].Equals(currenTypeString)) continue;
                this.index = i;
                return true;
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
                if (this.typeStrings[i].ToType() != typeString.ToType()) continue;
                this.index = i;
                return i;
            }
            return -1;
        }

        public ComponentBindInfo(Object target)
        {
            if (target == null) return;
            if (target is GameObject gameObject)
            {
                this.prefabObject = CommonTools.GetPrefabAsset(gameObject);
                this.instanceObject = gameObject;
                AddComponentsTypes(gameObject);
            }
            else if (target is Component component)
            {
                this.prefabObject = CommonTools.GetPrefabAsset(component.gameObject);
                this.instanceObject = component.gameObject;
                AddComponentsTypes(component.gameObject);
            }
            else
            {
                Type type = target.GetType();
                this.typeStrings = new TypeString[] {new TypeString(type)};
                this.index = 0;
            }
        }

        private void AddComponentsTypes(GameObject gameObject)
        {
            List<TypeString> typeStringList = new List<TypeString>();

            Type gameObjectType = typeof(GameObject);
            TypeString gameObjectTypeString = new TypeString(gameObjectType);
            typeStringList.Add(gameObjectTypeString);

            Component[] cs = gameObject.GetComponents(typeof(Component));
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