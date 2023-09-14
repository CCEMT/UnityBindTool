using System;
using BindTool;
using UnityEngine;

public partial class BindWindow
{
    public void SelectBindData(BindData bindData)
    {
        this.selectBindDataList.Clear();
        this.selectbindCollectionList.Clear();
        selectBindAmount = 1;
        selectBindDataList.Add(bindData);
        searchType = SearchType.All;
        bindTypeIndex = BindTypeIndex.Item;
        Repaint();
    }

    void SearchSelectList()
    {
        if (this.showAmount == 0) { return; }
        this.selectBindDataList.Clear();
        this.selectbindCollectionList.Clear();

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

        this.selectBindAmount = selectBindDataList.Count;
        this.selectbindCollectionAmount = this.selectbindCollectionList.Count;

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
        if (selectBindAmount == 0)
        {
            this.maxIndex = 0;
            this.currentIndex = 0;
            return;
        }

        this.maxIndex = (int) Math.Ceiling(this.selectBindAmount / (double) this.showAmount);
        this.currentIndex = Mathf.Clamp(this.currentIndex, 1, this.maxIndex);
    }

    void CollectionIndexSetting()
    {
        if (selectbindCollectionAmount == 0)
        {
            this.maxIndex = 0;
            this.currentIndex = 0;
            return;
        }

        this.maxIndex = (int) Math.Ceiling(this.selectbindCollectionAmount / (double) this.showAmount);
        this.currentIndex = Mathf.Clamp(this.currentIndex, 1, this.maxIndex);
    }

    void SearchAll()
    {
        switch (bindTypeIndex)
        {
            case BindTypeIndex.Item:
                SearchItem();
                break;
            case BindTypeIndex.Collection:
                SearchCollection();
                break;
        }
    }

    void SearchItem()
    {
        int searchAmount = this.editorObjectInfo.bindDataList.Count;
        for (int i = 0; i < searchAmount; i++)
        {
            BindData bindData = this.editorObjectInfo.bindDataList[i];
            if (CommonTools.Search(bindData.GetTypeName(), this.bindInputString))
            {
                this.selectBindDataList.Add(bindData);
                continue;
            }

            if (CommonTools.Search(bindData.GetValue().name, this.bindInputString))
            {
                this.selectBindDataList.Add(bindData);
                continue;
            }

            if (CommonTools.Search(bindData.name, this.bindInputString))
            {
                this.selectBindDataList.Add(bindData);
                continue;
            }
        }
    }

    void SearchCollection()
    {
        int searchAmount = this.editorObjectInfo.bindCollectionList.Count;
        for (int i = 0; i < searchAmount; i++)
        {
            BindCollection bindCollection = this.editorObjectInfo.bindCollectionList[i];
            if (CommonTools.Search(bindCollection.GetTypeString().typeName, this.bindInputString))
            {
                this.selectbindCollectionList.Add(bindCollection);
                continue;
            }

            if (CommonTools.Search(bindCollection.collectionType.ToString(), this.bindInputString))
            {
                this.selectbindCollectionList.Add(bindCollection);
                continue;
            }

            if (CommonTools.Search(bindCollection.name, this.bindInputString))
            {
                this.selectbindCollectionList.Add(bindCollection);
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
        int searchAmount = this.editorObjectInfo.bindDataList.Count;
        for (int i = 0; i < searchAmount; i++)
        {
            BindData bindData = this.editorObjectInfo.bindDataList[i];
            if (! CommonTools.Search(bindData.GetTypeName(), this.bindInputString)) continue;
            this.selectBindDataList.Add(bindData);
        }
    }

    void SearchTypeNameCollection()
    {
        int searchAmount = this.editorObjectInfo.bindCollectionList.Count;
        for (int i = 0; i < searchAmount; i++)
        {
            BindCollection bindCollection = this.editorObjectInfo.bindCollectionList[i];
            if (! CommonTools.Search(bindCollection.GetTypeString().typeName, this.bindInputString)) continue;
            this.selectbindCollectionList.Add(bindCollection);
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
        int searchAmount = this.editorObjectInfo.bindDataList.Count;
        for (int i = 0; i < searchAmount; i++)
        {
            BindData bindData = this.editorObjectInfo.bindDataList[i];
            if (! CommonTools.Search(bindData.GetValue().name, this.bindInputString)) continue;
            this.selectBindDataList.Add(bindData);
        }
    }

    void SearchGameObjectNameCollection()
    {
        int searchAmount = this.editorObjectInfo.bindCollectionList.Count;
        for (int i = 0; i < searchAmount; i++)
        {
            BindCollection bindCollection = this.editorObjectInfo.bindCollectionList[i];
            if (! CommonTools.Search(bindCollection.collectionType.ToString(), this.bindInputString)) continue;
            this.selectbindCollectionList.Add(bindCollection);
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
        int searchAmount = this.editorObjectInfo.bindDataList.Count;
        for (int i = 0; i < searchAmount; i++)
        {
            BindData bindData = this.editorObjectInfo.bindDataList[i];
            if (! CommonTools.Search(bindData.name, this.bindInputString)) continue;
            this.selectBindDataList.Add(bindData);
        }
    }

    void SearchVariableNameCollection()
    {
        int searchAmount = this.editorObjectInfo.bindCollectionList.Count;
        for (int i = 0; i < searchAmount; i++)
        {
            BindCollection bindCollection = this.editorObjectInfo.bindCollectionList[i];
            if (! CommonTools.Search(bindCollection.name, this.bindInputString)) continue;
            this.selectbindCollectionList.Add(bindCollection);
        }
    }
}