using System;
using BindTool;
using Sirenix.Utilities.Editor;
using UnityEngine;

public partial class BindWindow
{
    void SearchSelectList()
    {
        if (this.showAmount == 0) { return; }
        this.selectComponentList.Clear();
        this.selectDataList.Clear();
        this.selectComponentCollectionList.Clear();
        this.selectDataCollectionList.Clear();

        switch (this.searchType)
        {
            case SearchType.All:
                SearchAll();
                break;
            case SearchType.TypeName:
                SearchTypeName();
                break;
            case SearchType.TargetName:
                SearchGameObjectName();
                break;
            case SearchType.Name:
                SearchVariableName();
                break;
        }

        this.selectComponentAmount = this.selectComponentList.Count;
        this.selectDataAmount = this.selectDataList.Count;
        this.selectComponentCollectionAmount = this.selectComponentCollectionList.Count;
        this.selectDataCollectionAmount = this.selectDataCollectionList.Count;

        switch (this.bindTypeIndex)
        {
            case BindTypeIndex.Item:
                ItemIndexSetting();
                break;
            case BindTypeIndex.Collection:
                CollectionIndexSetting();
                break;
        }
    }

    void ItemIndexSetting()
    {
        if (this.selectComponentAmount + this.selectDataAmount == 0)
        {
            this.maxIndex = 0;
            this.currentIndex = 0;
            return;
        }

        this.maxIndex = (int) Math.Ceiling((this.selectComponentAmount + this.selectDataAmount)/ (double) this.showAmount);
        this.currentIndex = Mathf.Clamp(this.currentIndex, 1, this.maxIndex);
    }

    void CollectionIndexSetting()
    {
        if (this.selectComponentCollectionAmount + this.selectDataCollectionAmount == 0)
        {
            this.maxIndex = 0;
            this.currentIndex = 0;
            return;
        }

        this.maxIndex = (int) Math.Ceiling((this.selectComponentCollectionAmount + this.selectDataCollectionAmount) / (double) this.showAmount);
        this.currentIndex = Mathf.Clamp(this.currentIndex, 1, this.maxIndex);
    }

    void SearchAll()
    {
        switch (bindTypeIndex)
        {
            case BindTypeIndex.Item:
                SearchAllItem();
                break;
            case BindTypeIndex.Collection:
                SearchAllCollection();
                break;
        }
    }

    void SearchAllItem()
    {
        int bindComponentAmount = this.editorObjectInfo.gameObjectBindInfoList.Count;
        int bindDataAmount = this.editorObjectInfo.dataBindInfoList.Count;

        for (int i = 0; i < bindComponentAmount; i++)
        {
            ComponentBindInfo info = this.editorObjectInfo.gameObjectBindInfoList[i];
            if (CommonTools.Search(info.GetTypeName(), this.bindInputString))
            {
                this.selectComponentList.Add(info);
                continue;
            }

            if (CommonTools.Search(info.GetObject().name, this.bindInputString))
            {
                this.selectComponentList.Add(info);
                continue;
            }

            if (CommonTools.Search(info.name, this.bindInputString))
            {
                this.selectComponentList.Add(info);
                continue;
            }
        }
        for (int i = 0; i < bindDataAmount; i++)
        {
            DataBindInfo info = this.editorObjectInfo.dataBindInfoList[i];

            if (CommonTools.Search(info.typeString.typeName, this.bindInputString))
            {
                this.selectDataList.Add(info);
                continue;
            }
            if (CommonTools.Search(info.bindObject.name, this.bindInputString))
            {
                this.selectDataList.Add(info);
                continue;
            }

            if (CommonTools.Search(info.name, this.bindInputString))
            {
                this.selectDataList.Add(info);
                continue;
            }
        }
    }

    void SearchAllCollection()
    {
        int componentCollectionAmount = this.editorObjectInfo.componentCollectionBindInfoList.Count;
        int dataCollectionAmount = this.editorObjectInfo.dataCollectionBindInfoList.Count;

        for (int i = 0; i < componentCollectionAmount; i++)
        {
            ComponentCollectionBindInfo info = this.editorObjectInfo.componentCollectionBindInfoList[i];
            if (CommonTools.Search(info.GetTypeString().typeName, this.bindInputString))
            {
                this.selectComponentCollectionList.Add(info);
                continue;
            }

            if (CommonTools.Search(info.collectionType.ToString(), this.bindInputString))
            {
                this.selectComponentCollectionList.Add(info);
                continue;
            }

            if (CommonTools.Search(info.name, this.bindInputString))
            {
                this.selectComponentCollectionList.Add(info);
                continue;
            }
        }
        for (int i = 0; i < dataCollectionAmount; i++)
        {
            DataCollectionBindInfo info = this.editorObjectInfo.dataCollectionBindInfoList[i];

            if (CommonTools.Search(info.targetType.typeName, this.bindInputString))
            {
                this.selectDataCollectionList.Add(info);
                continue;
            }
            if (CommonTools.Search(info.collectionType.ToString(), this.bindInputString))
            {
                this.selectDataCollectionList.Add(info);
                continue;
            }

            if (CommonTools.Search(info.name, this.bindInputString))
            {
                this.selectDataCollectionList.Add(info);
                continue;
            }
        }
    }

    void SearchTypeName()
    {
        switch (bindTypeIndex)
        {
            case BindTypeIndex.Item:
                SearchTypeNameItem();
                break;
            case BindTypeIndex.Collection:
                SearchTypeNameCollection();
                break;
        }
    }

    void SearchTypeNameItem()
    {
        int bindComponentAmount = this.editorObjectInfo.gameObjectBindInfoList.Count;
        int bindDataAmount = this.editorObjectInfo.dataBindInfoList.Count;

        for (int i = 0; i < bindComponentAmount; i++)
        {
            ComponentBindInfo info = this.editorObjectInfo.gameObjectBindInfoList[i];
            if (! CommonTools.Search(info.GetTypeName(), this.bindInputString)) continue;
            this.selectComponentList.Add(info);
        }
        for (int i = 0; i < bindDataAmount; i++)
        {
            DataBindInfo info = this.editorObjectInfo.dataBindInfoList[i];

            if (! CommonTools.Search(info.typeString.typeName, this.bindInputString)) continue;
            this.selectDataList.Add(info);
        }
    }

    void SearchTypeNameCollection()
    {
        int componentCollectionAmount = this.editorObjectInfo.componentCollectionBindInfoList.Count;
        int dataCollectionAmount = this.editorObjectInfo.dataCollectionBindInfoList.Count;

        for (int i = 0; i < componentCollectionAmount; i++)
        {
            ComponentCollectionBindInfo info = this.editorObjectInfo.componentCollectionBindInfoList[i];
            if (! CommonTools.Search(info.GetTypeString().typeName, this.bindInputString)) continue;
            this.selectComponentCollectionList.Add(info);
        }
        for (int i = 0; i < dataCollectionAmount; i++)
        {
            DataCollectionBindInfo info = this.editorObjectInfo.dataCollectionBindInfoList[i];

            if (! CommonTools.Search(info.targetType.typeName, this.bindInputString)) continue;
            this.selectDataCollectionList.Add(info);
        }
    }

    void SearchGameObjectName()
    {
        switch (bindTypeIndex)
        {
            case BindTypeIndex.Item:
                SearchGameObjectNameItem();
                break;
            case BindTypeIndex.Collection:
                SearchGameObjectNameCollection();
                break;
        }
    }

    void SearchGameObjectNameItem()
    {
        int bindComponentAmount = this.editorObjectInfo.gameObjectBindInfoList.Count;
        int bindDataAmount = this.editorObjectInfo.dataBindInfoList.Count;

        for (int i = 0; i < bindComponentAmount; i++)
        {
            ComponentBindInfo info = this.editorObjectInfo.gameObjectBindInfoList[i];

            if (! CommonTools.Search(info.GetObject().name, this.bindInputString)) continue;
            this.selectComponentList.Add(info);
        }
        for (int i = 0; i < bindDataAmount; i++)
        {
            DataBindInfo info = this.editorObjectInfo.dataBindInfoList[i];

            if (! CommonTools.Search(info.bindObject.name, this.bindInputString)) continue;
            this.selectDataList.Add(info);
        }
    }

    void SearchGameObjectNameCollection()
    {
        int componentCollectionAmount = this.editorObjectInfo.componentCollectionBindInfoList.Count;
        int dataCollectionAmount = this.editorObjectInfo.dataCollectionBindInfoList.Count;

        for (int i = 0; i < componentCollectionAmount; i++)
        {
            ComponentCollectionBindInfo info = this.editorObjectInfo.componentCollectionBindInfoList[i];

            if (! CommonTools.Search(info.collectionType.ToString(), this.bindInputString)) continue;
            this.selectComponentCollectionList.Add(info);
        }
        for (int i = 0; i < dataCollectionAmount; i++)
        {
            DataCollectionBindInfo info = this.editorObjectInfo.dataCollectionBindInfoList[i];

            if (! CommonTools.Search(info.collectionType.ToString(), this.bindInputString)) continue;
            this.selectDataCollectionList.Add(info);
        }
    }

    void SearchVariableName()
    {
        switch (bindTypeIndex)
        {
            case BindTypeIndex.Item:
                SearchVariableNameItem();
                break;
            case BindTypeIndex.Collection:
                SearchVariableNameCollection();
                break;
        }
    }

    void SearchVariableNameItem()
    {
        int bindComponentAmount = this.editorObjectInfo.gameObjectBindInfoList.Count;
        int bindDataAmount = this.editorObjectInfo.dataBindInfoList.Count;

        for (int i = 0; i < bindComponentAmount; i++)
        {
            ComponentBindInfo info = this.editorObjectInfo.gameObjectBindInfoList[i];

            if (! CommonTools.Search(info.name, this.bindInputString)) continue;
            this.selectComponentList.Add(info);
        }
        for (int i = 0; i < bindDataAmount; i++)
        {
            DataBindInfo info = this.editorObjectInfo.dataBindInfoList[i];

            if (! CommonTools.Search(info.name, this.bindInputString)) continue;
            this.selectDataList.Add(info);
        }
    }

    void SearchVariableNameCollection()
    {
        int componentCollectionAmount = this.editorObjectInfo.componentCollectionBindInfoList.Count;
        int dataCollectionAmount = this.editorObjectInfo.dataCollectionBindInfoList.Count;

        for (int i = 0; i < componentCollectionAmount; i++)
        {
            ComponentCollectionBindInfo info = this.editorObjectInfo.componentCollectionBindInfoList[i];

            if (! CommonTools.Search(info.name, this.bindInputString)) continue;
            this.selectComponentCollectionList.Add(info);
        }
        for (int i = 0; i < dataCollectionAmount; i++)
        {
            DataCollectionBindInfo info = this.editorObjectInfo.dataCollectionBindInfoList[i];

            if (! CommonTools.Search(info.name, this.bindInputString)) continue;
            this.selectDataCollectionList.Add(info);
        }
    }
}