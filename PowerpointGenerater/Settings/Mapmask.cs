using ISettings;

namespace PowerpointGenerater
{
    public class Mapmask : IMapmask
    {
        public string Name { get; set; }
        public string RealName { get; set; }

        public Mapmask(string name, string realName)
        {
            Name = name;
            RealName = realName;
        }

        public override string ToString()
        {
            return $"Naam:{RealName}, Virtuele Naam:{Name}";
        }
    }
}
