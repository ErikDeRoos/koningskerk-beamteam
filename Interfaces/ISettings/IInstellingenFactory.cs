
namespace ISettings
{
    public interface IInstellingenFactory
    {
        bool WriteToXmlFile(IInstellingen instellingen);
        IInstellingen LoadFromXmlFile();
    }
}
