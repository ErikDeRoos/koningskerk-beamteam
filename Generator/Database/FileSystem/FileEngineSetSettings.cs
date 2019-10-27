// Copyright 2016 door Erik de Roos
using IDatabase;

namespace Generator.Database.FileSystem
{
    public class FileEngineSetSettings : DbSetSettings
    {
        public string DisplayName { get; set; }

        public bool UseContainer { get; set; }

        public bool ItemsHaveSubContent { get; set; }
        public bool ItemIsSubContent { get; set; }

        public bool NotVisibleInIndex { get; set; }
    }
}
