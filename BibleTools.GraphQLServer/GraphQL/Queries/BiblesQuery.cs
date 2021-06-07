using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using HotChocolate.Types;

namespace BibleTools.GraphQLServer.GraphQL.Queries
{
    public class BiblesQuery : QueryField
    {
        private async Task<Bible?> GetOsisBible(string id, CancellationToken cancellationToken)
        {
            var language = await Osis.GetLanguageForId(id, cancellationToken);

            if (language == null)
                return null;

            var osis = Osis.Get(language, id);

            if (osis == null)
                return null;

            var bible = new Bible(
                bibleType: BibleTypeEnum.Osis,
                id: id,
                title: Osis.GetTitle(osis) ?? throw new NullReferenceException("title"),
                abbreviation: Osis.GetAbbreviation(osis) ?? id
            );

            bible.Language = CultureInfo.GetCultureInfo(language);

            return bible;
        }

        protected override void ConfigureQuery(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field("bible")
                .Argument("type", argument => argument.Type<NonNullType<BibleTypeEnumType>>())
                .Argument("id", argument => argument.Type<NonNullType<StringType>>())
                .Resolve<Bible?>(async context =>
                {
                    var type = context.ArgumentValue<BibleTypeEnum>("type");

                    switch (type)
                    {
                        case BibleTypeEnum.Osis:
                            return await GetOsisBible(context.ArgumentValue<string>("id"), context.RequestAborted);
                        // case BibleTypeEnum.Zefania:
                        default:
                            throw new NotImplementedException($"type '${type}'");
                    }
                });
        }
    }

    public class BibleType : ObjectType<Bible>
    {
        protected override void Configure(IObjectTypeDescriptor<Bible> descriptor)
        {
            descriptor.BindFieldsImplicitly();

            descriptor.Field(t => t.Language)
                .Name("language")
                .Type<StringType>()
                .Resolve<string?>(context => context.Parent<Bible>()?.Language?.EnglishName);
        }
    }

    public class BibleTypeEnumType : EnumType<BibleTypeEnum>
    {
        protected override void Configure(IEnumTypeDescriptor<BibleTypeEnum> descriptor)
        {
            descriptor.Name("BibleType");

            descriptor.BindValuesImplicitly();
        }
    }

    public class Bible
    {
        public Bible(BibleTypeEnum bibleType, string id, string title, string abbreviation)
        {
            this.BibleType = bibleType;
            this.Id = id;
            this.Title = title;
            this.Abbreviation = abbreviation;
        }

        public BibleTypeEnum BibleType { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Abbreviation { get; set; }

        public CultureInfo? Language { get; set; }
    }

    public enum BibleTypeEnum
    {
        Osis,
        // Zefania
    }
}