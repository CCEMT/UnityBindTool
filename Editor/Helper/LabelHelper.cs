using BindTool;

public static class LabelHelper
{
    public static string GetVisitString(VisitType valueTuple)
    {
        switch (valueTuple)
        {
            case VisitType.Public:
                return "public";
            case VisitType.Private:
                return "private";
            case VisitType.Protected:
                return "protected";
            case VisitType.Internal:
                return "internal";
        }
        return null;
    }

    public static string GetRemoveString(RemoveType removeType)
    {
        switch (removeType)
        {
            case RemoveType.This:
                return "解除自身绑定";
            case RemoveType.Child:
                return "解除子对象绑定";
            case RemoveType.ThisAndChild:
                return "解除自身以及子对象绑定";
        }
        return null;
    }
}