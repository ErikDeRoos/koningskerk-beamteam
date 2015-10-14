namespace PowerpointGenerater
{
    public class Mapmask
    {
        public string Name { get; set; }
        public string RealName { get; set; }

        public Mapmask(string Name, string RealName)
        {
            this.Name = Name;
            this.RealName = RealName;
        }

        public override string ToString()
        {
            return string.Format("Naam:{0}, Virtuele Naam:{1}", RealName, Name);
        }
    }
}
