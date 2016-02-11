using IDatabase;

namespace PowerpointGenerater.Database
{
    public class FileEngineSetSettings : ISetSettings
    {
        public string DisplayName { get; set; }

        public bool ItemsHaveSubContent { get; set; }

        public bool UseContainer { get; set; }
    }
}
