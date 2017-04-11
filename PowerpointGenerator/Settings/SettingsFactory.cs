// Copyright 2016 door Remco Veurink en Erik de Roos
using IFileSystem;
using ISettings;
using ISettings.CommonImplementation;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace PowerpointGenerator.Settings
{
    public class SettingsFactory : IInstellingenFactory
    {
        private readonly string _baseDir;
        private readonly string _instellingenFileName;
        private readonly string _masksFileName;
        private IFileOperations _fileManager;

        public SettingsFactory(IFileOperations fileManager, string instellingenFileName, string masksFileName)
        {
            _fileManager = fileManager;
            _baseDir = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);  // TODO alternatief voor vinden
            _instellingenFileName = instellingenFileName;
            _masksFileName = masksFileName;
        }

        public bool WriteToFile(IInstellingen instellingen)
        {
            return WriteToJsonFile(_fileManager, _fileManager.CombineDirectories(_baseDir, _instellingenFileName), _fileManager.CombineDirectories(_baseDir, _masksFileName), (instellingen as Instellingen) ?? GetDefault());
        }

        public IInstellingen LoadFromFile()
        {
            var settingsFromJson = LoadFromJsonFile(_fileManager, _fileManager.CombineDirectories(_baseDir, _instellingenFileName), _fileManager.CombineDirectories(_baseDir, _masksFileName));
            if (settingsFromJson != null)
                return settingsFromJson;
            return GetDefault();
        }

        private static bool WriteToJsonFile(IFileOperations fileManager, string instellingenFile, string maskFile, Instellingen instellingen)
        {
            try
            {
                // verwijder oude bestand (zelfde effect als overschreven worden)
                if (fileManager.FileExists(instellingenFile))
                    fileManager.Delete(instellingenFile);

                //schrijf instellingen weg
                using (var sw = new StreamWriter(fileManager.FileWriteStream(instellingenFile)))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(sw, instellingen);
                    sw.Flush();
                }

                //schrijf Masks weg
                using (var sw = new StreamWriter(fileManager.FileWriteStream(maskFile)))
                {
                    var serializer = new JsonSerializer();
                    var maskArray = instellingen.Masks.Select(m => new Mask() { Name = m.Name, RealName = m.RealName }).ToArray();
                    serializer.Serialize(sw, maskArray);
                    sw.Flush();
                }

                return true;
            }
            catch (PathTooLongException)
            {
                return false;
            }
        }

        private static Instellingen LoadFromJsonFile(IFileOperations fileManager, string instellingenFile, string maskFile)
        {
            Instellingen instellingen;

            if (!fileManager.FileExists(instellingenFile))
                return null;

            using (var file = new StreamReader(fileManager.FileReadStream(instellingenFile)))
            {
                var serializer = new JsonSerializer();
                instellingen = (Instellingen)serializer.Deserialize(file, typeof(Instellingen));
            }
            if (instellingen == null)
                return null;

            if (!fileManager.FileExists(maskFile))
                return instellingen;

            using (var file = new StreamReader(fileManager.FileReadStream(maskFile)))
            {
                var serializer = new JsonSerializer();
                foreach (var mask in (Mask[])serializer.Deserialize(file, typeof(Mask[])))
                {
                    instellingen.AddMask(new Mapmask(mask.Name, mask.RealName));
                }
            }

            return instellingen;
        }

        private static Instellingen GetDefault()
        {
            return new Instellingen() {
                DatabasePad = @".Resources\Database",
                BijbelPad = @".Resources\Bijbels\NBV",
                TemplateTheme = @".Resources\Database\Achtergrond.pptx",
                TemplateLied = @".Resources\Database\Template Liederen.pptx",
                TemplateBijbeltekst = @".Resources\Database\Template Bijbeltekst.pptx",
            };
        }

        private class Mask
        {
            public string Name { get; set; }
            public string RealName { get; set; }
        }
    }
}
