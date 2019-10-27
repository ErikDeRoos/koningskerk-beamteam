// Copyright 2019 door Erik de Roos

namespace IDatabase.Engine
{
    public interface IEngineSelection
    {
        string Name { get; }
        IEngine Engine { get; }
    }
}
