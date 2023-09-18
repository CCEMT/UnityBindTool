namespace UnityBindTool
{
    public static class RepetitionNameDisposeFactory
    {
        public static IRepetitionNameDisposer GetRepetitionNameDisposer(RepetitionNameDispose nameDispose)
        {
            switch (nameDispose)
            {
                case RepetitionNameDispose.AddNumber:
                    return new NameAddNumberDisposer();
            }
            return new NoneNameDisposer();
        }
    }
}