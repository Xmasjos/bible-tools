using System.Collections.Generic;
using System.Globalization;

namespace BibleTools.GraphQLServer.Models
{
    public class BibleLanguage
    {
        public BibleLanguage(CultureInfo language, ICollection<Bible> bibles)
        {
            this.Language = language;
            this.Bibles = bibles;
        }

        public CultureInfo Language { get; set; }
        public ICollection<Bible> Bibles { get; set; }
    }
}