using IDatabase;

namespace Generator.Database.FileSystem
{
    public class FileEngineSetSettings : ISetSettings
    {
        public string DisplayName { get; set; }

        public bool ItemsHaveSubContent { get; set; }

        public bool UseContainer { get; set; }
    }
}
