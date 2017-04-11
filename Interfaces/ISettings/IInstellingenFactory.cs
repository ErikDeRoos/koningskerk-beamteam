// Copyright 2016 door Erik de Roos

namespace ISettings
{
    public interface IInstellingenFactory
    {
        bool WriteToFile(IInstellingen instellingen);
        IInstellingen LoadFromFile();
    }
}
