using System.Globalization;

namespace BibleTools.GraphQLServer.Models
{
    public class Bible
    {
        public Bible(BibleTypeEnum bibleType, string id, string title)
        {
            this.BibleType = bibleType;
            this.Id = id;
            this.Title = title;
        }

        public BibleTypeEnum BibleType { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Abbreviation { get; set; }

        public CultureInfo? Language { get; set; }
    }
}