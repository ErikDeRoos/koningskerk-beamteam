// Copyright 2019 door Erik de Roos

namespace Generator.Database.FileSystem
{
    public interface IEngineSelection
    {
        string Name { get; }
        IEngine Engine { get; }
    }
}
