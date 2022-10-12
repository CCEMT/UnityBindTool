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
    public class ObjectInfo
    {
        public TypeString typeString;
        public ComponentBindInfo rootBindInfo;
        public List<ComponentBindInfo> gameObjectBindInfoList;
        public List<DataBindInfo> dataBindInfoList;

        public void SetObject(GameObject instanceObject)
        {
            rootBindInfo = new ComponentBindInfo(instanceObject);
        }

        public void AgainGet()
        {
            if (rootBindInfo.AgainGet() == false)
            {
                if (rootBindInfo.autoBindSetting != null) ComponentBind(rootBindInfo, rootBindInfo.autoBindSetting);
            }

            int amount = gameObjectBindInfoList.Count;
            for (int i = 0; i < amount; i++)
            {
                var info = gameObjectBindInfoList[i];
                if (info.AgainGet() == false)
                {
                    if (info.autoBindSetting != null) ComponentBind(info, info.autoBindSetting);
                }
            }
        }

        public ComponentBindInfo AutoBind(Object ob, AutoBindSetting autoBindSetting)
        {
            ComponentBindInfo componentBindInfo = new ComponentBindInfo(ob);
            BindByAutoSetting(componentBindInfo, autoBindSetting);
            return componentBindInfo;
        }

        public void AutoBind(ComponentBindInfo bindInfo, AutoBindSetting autoBindSetting)
        {
            BindByAutoSetting(bindInfo, autoBindSetting);
        }

        void BindByAutoSetting(ComponentBindInfo bindInfo, AutoBindSetting autoBindSetting)
        {
            var bindObject = bindInfo.GetObject();
            if (bindObject == null) return;
            int lgnoreAmount = autoBindSetting.nameLgnoreDataList.Count;
            for (int i = 0; i < lgnoreAmount; i++)
            {
                var data = autoBindSetting.nameLgnoreDataList[i];
                if (data.Check(bindObject.name, out string _)) return;
            }
            ComponentBind(bindInfo, autoBindSetting);

        }

        void ComponentBind(ComponentBindInfo bindInfo, AutoBindSetting autoBindSetting)
        {
            bindInfo.autoBindSetting = autoBindSetting;
            var bindObject = bindInfo.GetObject();

            List<TypeString> tempTypeList = new List<TypeString>();
            tempTypeList.AddRange(bindInfo.typeStrings);

            int nameBindAmont = autoBindSetting.nameBindDataList.Count;
            for (int i = 0; i < nameBindAmont; i++)
            {
                var data = autoBindSetting.nameBindDataList[i];
                if (data.nameCheck.Check(bindObject.name, out string _))
                {
                    if (tempTypeList.Contains(data.typeString))
                    {
                        var index = bindInfo.SetIndex(data.typeString);
                        if (index != -1)
                        {
                            this.gameObjectBindInfoList.Add(bindInfo);
                            return;
                        }
                    }
                }
            }

            if (autoBindSetting.isEnableStreamingBind)
            {
                List<TypeString> elseType = new List<TypeString>();
                elseType.AddRange(tempTypeList);
                elseType.Remove(new TypeString(typeof(GameObject)));
                autoBindSetting.streamingBindDataList = autoBindSetting.streamingBindDataList.OrderByDescending(x => x.sequence).ToList();
                int sequenceAmount = autoBindSetting.streamingBindDataList.Count;
                for (int i = 0; i < sequenceAmount; i++)
                {
                    var data = autoBindSetting.streamingBindDataList[i];
                    if (tempTypeList.Contains(data.typeString)) elseType.Remove(data.typeString);
                }

                for (int i = 0; i < sequenceAmount; i++)
                {
                    var data = autoBindSetting.streamingBindDataList[i];
                    if (data.isElse)
                    {
                        if (elseType.Count > 0)
                        {
                            bindInfo.SetIndex(elseType.First());
                            break;
                        }
                    }
                    else
                    {
                        if (tempTypeList.Contains(data.typeString))
                        {
                            bindInfo.SetIndex(data.typeString);
                            break;
                        }
                    }
                }

                this.gameObjectBindInfoList.Add(bindInfo);
            }
            else
            {
                if (autoBindSetting.isBindComponent)
                {
                    if (autoBindSetting.isBindAllComponent)
                    {
                        int amount = bindInfo.typeStrings.Length;
                        for (int i = 0; i < amount; i++)
                        {
                            ComponentBindInfo componentBindInfo = new ComponentBindInfo(bindInfo.instanceObject);
                            componentBindInfo.index = i;
                            this.gameObjectBindInfoList.Add(componentBindInfo);
                        }
                    }
                    else { this.gameObjectBindInfoList.Add(bindInfo); }
                }
            }
        }

        public void Bind(ComponentBindInfo bindInfo, int index)
        {
            bindInfo.index = index;
            gameObjectBindInfoList.Add(bindInfo);
        }

        public ComponentBindInfo AddObject(GameObject instanceObject)
        {
            ComponentBindInfo componentBindInfo = new ComponentBindInfo(instanceObject);
            gameObjectBindInfoList.Add(componentBindInfo);
            return componentBindInfo;
        }

        #region Equals

        public bool GameObjectEquals(object obj)
        {
            if (obj != null)
            {
                GameObject targetObject = obj as GameObject;
                if (targetObject != null)
                {
                    if (targetObject == rootBindInfo.instanceObject) { return true; }
                    else if (targetObject == rootBindInfo.prefabObject) return true;

                    GameObject targetPrefab = CommonTools.GetPrefabAsset(targetObject);
                    GameObject currentPrefab = CommonTools.GetPrefabAsset(rootBindInfo.GetObject());
                    if (targetPrefab == currentPrefab) return true;
                }
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            ObjectInfo objectInfo = (ObjectInfo) obj;
            if (objectInfo != null) return Equals(objectInfo);
            return false;
        }

        protected bool Equals(ObjectInfo other)
        {
            return typeString.Equals(other.typeString) && Equals(rootBindInfo, other.rootBindInfo) && Equals(gameObjectBindInfoList, other.gameObjectBindInfoList) &&
                   Equals(dataBindInfoList, other.dataBindInfoList);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = typeString.GetHashCode();
                hashCode = (hashCode * 397) ^ (rootBindInfo != null ? rootBindInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (gameObjectBindInfoList != null ? gameObjectBindInfoList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (dataBindInfoList != null ? dataBindInfoList.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}