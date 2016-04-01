// Copyright 2016 door Erik de Roos

namespace IDatabase.Engine
{
    public interface IEngineSelection<T> where T : class, ISetSettings, new()
    {
        string Name { get; }
        IEngine<T> Engine { get; }
    }
}
