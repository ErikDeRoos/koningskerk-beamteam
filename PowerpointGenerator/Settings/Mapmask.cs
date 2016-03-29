// Copyright 2016 door Remco Veurink en Erik de Roos
using ISettings;

namespace PowerpointGenerator
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
