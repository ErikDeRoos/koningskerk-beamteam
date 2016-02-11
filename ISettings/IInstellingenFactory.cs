
namespace ISettings
{
    public interface IInstellingenFactory
    {
        bool WriteToXMLFile(IInstellingen instellingen);
        IInstellingen LoadFromXMLFile();
    }
}
