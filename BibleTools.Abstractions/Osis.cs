using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using Konves.Osis;
using Konves.Osis.ObjectModel;

using BibleTools.Extensions;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BibleTools
{
    public class Osis
    {
        // private static 
        public static string? GetAbbreviation(osisCT osis)
        {
            workCT[] workItems;

            if (osis.Item is osisCorpusCT osisCorpus)
                workItems = osisCorpus.header.work;
            else if (osis.Item is osisTextCT osisText)
                workItems = osisText.header.work;
            else
                throw new NotSupportedException(osis.Item.GetType().FullName);

            foreach (var workItem in workItems)
            {
                foreach (var idItem in workItem.identifier)
                {
                    if (!string.IsNullOrWhiteSpace(idItem.ID))
                        return idItem.ID;
                    if (idItem.osisID?.Any() == true)
                        return idItem.osisID.First();
                    if (!string.IsNullOrWhiteSpace(idItem.Value))
                        return idItem.Value;
                }
            }

            return null;
        }

        public static string? GetTitle(osisCT osis)
        {
            workCT[] workItems;

            if (osis.Item is osisCorpusCT osisCorpus)
                workItems = osisCorpus.header.work;
            else if (osis.Item is osisTextCT osisText)
                workItems = osisText.header.work;
            else
                throw new NotSupportedException(osis.Item.GetType().FullName);

            foreach (var workItem in workItems)
            {
                foreach (var titleItem in workItem.title)
                {
                    if (titleItem.Text.Any())
                        return titleItem.Text.First();
                }
            }

            return null;
        }

        public static string? GetTitle(FileInfo file)
        {
            if (!file.Exists)
                return null;

            var osisData = OsisSerializer.Deserialize(file.OpenRead());

            return GetTitle(osisData);
        }

        private static string GetRootPath()
        {
            var solutionPath = Assembly.GetExecutingAssembly().GetSolutionPath();

            if (solutionPath == null)
                throw new NullReferenceException(nameof(solutionPath));

            return Path.Combine(solutionPath, "bible", "osis");
        }

        public static osisCT? Get(string language, string id)
        {
            var path = $"{Path.Combine(GetRootPath(), language, id)}.xml";

            if (!File.Exists(path))
                return null;

            return OsisSerializer.Deserialize(File.OpenRead(path));
        }

        public static async Task<osisCT?> Get(string id, CancellationToken cancellationToken = default)
        {
            var language = await Osis.GetLanguageForId(id, cancellationToken);

            if (language == null)
                return null;

            return Get(language, id);
        }

        public static async Task<string?> GetLanguageForId(string id, CancellationToken cancellationToken = default)
        {
            var osisPath = GetRootPath();
            var indexPath = Path.Combine(osisPath, "index.xml");

            var indexXml = await XDocument.LoadAsync(File.OpenRead(indexPath), LoadOptions.None, cancellationToken);

            return indexXml.Descendants("bible")
                .Where(t => t.Attribute("id")?.Value == id)
                .Select(t => t.Parent?.Attribute("id")?.Value)
                .FirstOrDefault();
        }

        public static void UpdateIndex()
        {
            var osisPath = GetRootPath();
            var osisDirInfo = new DirectoryInfo(osisPath);

            var pathsByLanguage = osisDirInfo
                .EnumerateDirectories()
                .ToDictionary(t => CultureInfo.GetCultureInfo(t.Name), t => t);

            using var xmlWriter = XmlWriter.Create(Path.Combine(osisPath, "index.xml"));

            xmlWriter.WriteStartElement("bibles");

            foreach (var languageKV in pathsByLanguage.OrderBy(t => t.Key.EnglishName))
            {
                xmlWriter.WriteStartElement("language");

                xmlWriter.WriteAttributeString("title", languageKV.Key.EnglishName);
                xmlWriter.WriteAttributeString("id", languageKV.Value.Name);

                var bibles = languageKV.Value.EnumerateFiles()
                    .Select(t => new
                    {
                        Id = Path.GetFileNameWithoutExtension(t.Name),
                        Title = GetTitle(t)
                    });

                foreach (var bible in bibles.OrderBy(t => t.Title))
                {
                    xmlWriter.WriteStartElement("bible");
                    xmlWriter.WriteAttributeString("title", bible.Title);
                    xmlWriter.WriteAttributeString("id", bible.Id);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }
    }
}