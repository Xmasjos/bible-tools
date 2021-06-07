using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

using Konves.Osis;
using Konves.Osis.ObjectModel;

namespace BibleTools.Manager
{
    class Program
    {
        public static string GetTitle(FileInfo file)
        {
            var osisData = OsisSerializer.Deserialize(file.OpenRead());
            workCT[] workItems;

            //[System.Xml.Serialization.XmlElementAttribute("osisCorpus", typeof(osisCorpusCT))]
            // [System.Xml.Serialization.XmlElementAttribute("osisText", typeof(osisTextCT))]
            if (osisData.Item is osisCorpusCT osisCorpus)
                workItems = osisCorpus.header.work;
            else if (osisData.Item is osisTextCT osisText)
                workItems = osisText.header.work;
            else
                throw new NotSupportedException(osisData.Item.GetType().FullName);

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

        public static void UpdateOsisIndex()
        {
            var solutionPath = Assembly.GetExecutingAssembly().GetSolutionPath();
            var osisPath = Path.Combine(solutionPath, "bible", "osis");
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

        static void Main(string[] args)
        {
            UpdateOsisIndex();
        }
    }
}
