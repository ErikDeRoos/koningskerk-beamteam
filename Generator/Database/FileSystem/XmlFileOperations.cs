// Copyright 2024 door Erik de Roos
using Generator.Database.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Generator.Tools
{
    public class XmlFileOperations : IFileOperations
    {
        private readonly IFileOperations _fileManager;
        private readonly string _startDir;
        private readonly XmlFileSettings _xmlFileSettings;

        private XmlDocument _xmlDoc;
        private IEnumerable<DirWrapper> _dirs;

        public XmlFileOperations(IFileOperations fileMananger, string startDir, XmlFileSettings xmlFileSettings) 
        {
            _fileManager = fileMananger;
            _startDir = startDir;
            _xmlFileSettings = xmlFileSettings;
        }

        private XmlDocument GetXmlDocument()
        {
            if (_xmlDoc != null) return _xmlDoc;
            
            _xmlDoc = new XmlDocument();

            var xmlFileName = _fileManager.GetFiles(_startDir).FirstOrDefault(f => Path.GetFileName(f).Equals(_xmlFileSettings.FileName, StringComparison.InvariantCultureIgnoreCase));
            if (xmlFileName != null)
            {
                using (var stream = _fileManager.FileReadStream(xmlFileName))
                {
                    _xmlDoc.Load(stream);
                }
            }

            return _xmlDoc;
        }

        public IEnumerable<DirWrapper> GetDirs()
        {
            if (_dirs != null) return _dirs;

            return _dirs = GetDirsInternal().ToArray();
        }
        private IEnumerable<DirWrapper> GetDirsInternal()
        {
            var doc = GetXmlDocument();
            var root = doc.DocumentElement;
            if (root == null || root.Name != _xmlFileSettings.RootNamespace)
                yield break;

            foreach (XmlElement node in root.GetElementsByTagName(_xmlFileSettings.SetNamespace))
            {
                yield return new DirWrapper(node, _startDir, _xmlFileSettings);
            }
        }

        public string CombineDirectories(string atPath, string otherPath)
        {
            return Path.Combine(atPath, otherPath);
        }

        public bool DirExists(string atPath)
        {
            if (_startDir == atPath)
            {
                return true;
            }

            var dirs = GetDirs();
            return dirs.Any(d => d.FullPath == atPath);
        }

        public bool FileExists(string fileName)
        {
            var dirs = GetDirs();

            return dirs.Any(d => d.FileExists(fileName));
        }

        public Stream FileReadStream(string filename)
        {
            if (_startDir == filename)
            {
                return null;
            }
            var dir = GetDirs().FirstOrDefault(d => d.IsPathInDir(filename));
            if (dir == null)
                return null;

            return dir.ReadFile(filename);
        }

        public Stream FileWriteStream(string filename)
        {
            // Do not write to the xml doc, just give a memory stream
            return new MemoryStream();
        }

        public IEnumerable<string> GetDirectories(string atPath)
        {
            if (_startDir == atPath)
            {
                return GetDirs().Select(d => d.FullPath);
            }

            return Array.Empty<string>();
        }

        public IEnumerable<string> GetFiles(string atPath)
        {
            if (_startDir == atPath)
            {
                return Array.Empty<string>();
            }
            var dir = GetDirs().FirstOrDefault(d => d.IsPathInDir(atPath));
            if (dir == null)
                return Array.Empty<string>();
            
            return dir.GetFiles().Select(d => d.FullPath);
        }

        public string GetTempFileName()
        {
            return _fileManager.GetTempFileName();
        }

        public void Delete(string fileName)
        {
            // Do not delete something in the xml file
        }

        public static XmlFileSettings CreateSettingsFromSettingString(string settingsString)
        {
            var settings = new XmlFileSettings();

            if (string.IsNullOrWhiteSpace(settingsString))
                return settings;

            var parts = settingsString.Split('|');
            if (parts.Length != 9 && parts[0].Equals("xml", StringComparison.InvariantCultureIgnoreCase))
                return settings;

            settings.FileName = parts[1];
            settings.RootNamespace = parts[2];
            settings.SetNamespace = parts[3];
            settings.SetNameAttr = parts[4];
            settings.PartNamespace = parts[5];
            settings.PartNameAttr = parts[6];
            settings.SubpartNamespace = parts[7];
            settings.SubpartNameAttr = parts[8];

            return settings;
        }

        public class XmlFileSettings
        {
            public string FileName { get; set; } = "bible.xml";
            public string RootNamespace { get; set; } = "bible";
            public string SetNamespace { get; set; } = "b";
            public string SetNameAttr { get; set; } = "n";
            public string PartNamespace { get; set; } = "c";
            public string PartNameAttr { get; set; } = "n";
            public string SubpartNamespace { get; set; } = "v";
            public string SubpartNameAttr { get; set; } = "n";
        }


        public class DirWrapper
        {
            public string Name { get; }
            public string FullPath { get; }
            
            private readonly XmlElement _xmlElement;
            private readonly XmlFileSettings _xmlFileSettings;
            private readonly string _settingFileName;
            private IEnumerable<FileWrapper> _files;

            public DirWrapper(XmlElement xmlElement, string parentDir, XmlFileSettings xmlFileSettings) 
            { 
                _xmlElement = xmlElement;
                _xmlFileSettings = xmlFileSettings;
                Name = _xmlElement.Attributes.GetNamedItem(_xmlFileSettings.SetNameAttr)?.InnerText;
                FullPath = Path.Combine(parentDir, Name);
                _settingFileName = Path.Combine(FullPath, FileEngineDefaults.SetSettingsName);
            }

            public IEnumerable<FileWrapper> GetFiles()
            {
                if (_files != null) return _files;

                return _files = GetFilesInernal().ToArray();
            }
            private IEnumerable<FileWrapper> GetFilesInernal()
            {
                foreach (XmlElement node in _xmlElement.GetElementsByTagName(_xmlFileSettings.PartNamespace))
                {
                    yield return new FileWrapper(node, FullPath, _xmlFileSettings);
                }
            }

            public bool IsPathInDir(string path)
            {
                return path.StartsWith(FullPath);
            }

            public bool FileExists(string fileName)
            {
                if (!IsPathInDir(fileName))
                    return false;

                if (_settingFileName == fileName)
                    return true;

                return GetFiles().Any(d => d.FullPath == fileName);
            }

            public Stream ReadFile(string fileName)
            {
                if (_settingFileName == fileName)
                    return GetSettingsFileContent();

                var file = GetFiles().FirstOrDefault(f => f.FullPath == fileName);
                if (file == null)
                    return null;

                return file.ReadContent();
            }

            public Stream GetSettingsFileContent()
            {
                return new MemoryStream(Encoding.Default.GetBytes(SettingsFileContent()));
            }
            private string SettingsFileContent()
            {
                return "<?xml version = \"1.0\" encoding=\"utf-8\"?>" +
                "<FileEngineSetSettings xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                $"  <DisplayName>{Name}</DisplayName>" +
                "  <ItemsHaveSubContent>false</ItemsHaveSubContent>" +
                "  <UseContainer>false</UseContainer>" +
                "  <ItemIsSubContent>true</ItemIsSubContent>" +
                "</FileEngineSetSettings>";
            }
        }

        public class FileWrapper
        {
            public string Name { get; }
            public string FullPath { get; }

            private readonly XmlElement _xmlElement;
            private readonly XmlFileSettings _xmlFileSettings;
            private string _correctedContent;

            public FileWrapper(XmlElement xmlElement, string parentDir, XmlFileSettings xmlFileSettings)
            {
                _xmlElement = xmlElement;
                _xmlFileSettings = xmlFileSettings;
                Name = _xmlElement.Attributes.GetNamedItem(_xmlFileSettings.PartNameAttr)?.InnerText + ".txt";
                FullPath = Path.Combine(parentDir, Name);
            }

            public string GetCorrectedContent()
            {
                if (_correctedContent != null)
                    return _correctedContent;

                var builder = new StringBuilder();
                foreach (XmlElement node in _xmlElement.GetElementsByTagName(_xmlFileSettings.SubpartNamespace))
                {
                    builder.Append(node.Attributes.GetNamedItem(_xmlFileSettings.SubpartNameAttr)?.InnerText)
                        .Append(" ")
                        .AppendLine(node.InnerText);
                }
                return _correctedContent = builder.ToString();
            }

            public Stream ReadContent()
            {
                return new MemoryStream(Encoding.Default.GetBytes(GetCorrectedContent()));
            }
        }
    }
}
