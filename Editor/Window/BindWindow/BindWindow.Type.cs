using Sirenix.OdinInspector;

public partial class BindWindow
{
    enum SearchType
    {
        [LabelText("搜索全部")]
        All,

        [LabelText("按类型搜素")]
        TypeName,

        [LabelText("按目标搜素")]
        TargetName,

        [LabelText("按名称搜素")]
        Name
    }

    enum BindTypeIndex
    {
        Item = 0,
        Collection = 1
    }
}