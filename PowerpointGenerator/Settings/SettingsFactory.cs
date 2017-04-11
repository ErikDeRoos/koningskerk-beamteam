// Copyright 2016 door Remco Veurink en Erik de Roos
using IFileSystem;
using ISettings;
using ISettings.CommonImplementation;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace PowerpointGenerator.Settings
{
    // TODO settings definieeren in een key-value array zodat je meer op kan slaan met minder risico's op incombatibiliteit
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

                var saveInstellingen = new SaveInstellingen()
                {
                    DatabasePad = instellingen.DatabasePad,
                    BijbelPad = instellingen.BijbelPad,
                    TemplateTheme = instellingen.TemplateTheme,
                    TemplateLied = instellingen.TemplateLied,
                    TemplateBijbeltekst = instellingen.TemplateBijbeltekst,
                    TekstChar_a_OnARow = instellingen.TekstChar_a_OnARow,
                    TekstFontName = instellingen.TekstFontName,
                    TekstFontPointSize = instellingen.TekstFontPointSize,
                    RegelsPerLiedSlide = instellingen.RegelsPerLiedSlide,
                    RegelsPerBijbeltekstSlide = instellingen.RegelsPerBijbeltekstSlide,
                    StandaardTeksten = instellingen.StandaardTeksten,
                };

                //schrijf instellingen weg
                using (var sw = new StreamWriter(fileManager.FileWriteStream(instellingenFile)))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(sw, saveInstellingen);
                    sw.Flush();
                }

                var saveMasks = instellingen.Masks.Select(m => new SaveMask() { Name = m.Name, RealName = m.RealName }).ToArray();
                
                //schrijf Masks weg
                using (var sw = new StreamWriter(fileManager.FileWriteStream(maskFile)))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(sw, saveMasks);
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
            SaveInstellingen saveInstellingen;

            if (!fileManager.FileExists(instellingenFile))
                return null;

            using (var file = new StreamReader(fileManager.FileReadStream(instellingenFile)))
            {
                var serializer = new JsonSerializer();
                saveInstellingen = (SaveInstellingen)serializer.Deserialize(file, typeof(SaveInstellingen));
            }
            if (saveInstellingen == null)
                return null;
            var instellingen = new Instellingen()
            {
                DatabasePad = saveInstellingen.DatabasePad,
                BijbelPad = saveInstellingen.BijbelPad,
                TemplateTheme = saveInstellingen.TemplateTheme,
                TemplateLied = saveInstellingen.TemplateLied,
                TemplateBijbeltekst = saveInstellingen.TemplateBijbeltekst,
                TekstChar_a_OnARow = saveInstellingen.TekstChar_a_OnARow,
                TekstFontName = saveInstellingen.TekstFontName,
                TekstFontPointSize = saveInstellingen.TekstFontPointSize,
                RegelsPerLiedSlide = saveInstellingen.RegelsPerLiedSlide,
                RegelsPerBijbeltekstSlide = saveInstellingen.RegelsPerBijbeltekstSlide,
                StandaardTeksten = saveInstellingen.StandaardTeksten,
            };

            if (!fileManager.FileExists(maskFile))
                return instellingen;

            using (var file = new StreamReader(fileManager.FileReadStream(maskFile)))
            {
                var serializer = new JsonSerializer();
                foreach (var mask in (SaveMask[])serializer.Deserialize(file, typeof(SaveMask[])))
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

        private class SaveMask
        {
            public string Name { get; set; }
            public string RealName { get; set; }
        }

        private class SaveInstellingen
        {
            public string DatabasePad { get; set; }
            public string BijbelPad { get; set; }
            public string TemplateTheme { get; set; }
            public string TemplateLied { get; set; }
            public string TemplateBijbeltekst { get; set; }
            public int TekstChar_a_OnARow { get; set; }
            public string TekstFontName { get; set; }
            public float TekstFontPointSize { get; set; }
            public int RegelsPerLiedSlide { get; set; }
            public int RegelsPerBijbeltekstSlide { get; set; }
            public StandaardTeksten StandaardTeksten { get; set; }
        }
    }
}
