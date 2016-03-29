// Copyright 2016 door Erik de Roos

namespace ISettings
{
    public interface IInstellingenFactory
    {
        bool WriteToXmlFile(IInstellingen instellingen);
        IInstellingen LoadFromXmlFile();
    }
}
