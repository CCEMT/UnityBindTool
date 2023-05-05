public class NoneNameDisposer : IRepetitionNameDisposer
{
    public string DisposeName(NameDisposeCentre nameDisposeCentre, string rawName)
    {
        nameDisposeCentre.useNames.Add(rawName);
        return rawName;
    }
}