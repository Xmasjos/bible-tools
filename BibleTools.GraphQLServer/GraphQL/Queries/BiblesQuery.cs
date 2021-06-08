using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using HotChocolate.Types;

using BibleTools.GraphQLServer.GraphQL.Types;
using BibleTools.GraphQLServer.Models;

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
                title: Osis.GetTitle(osis) ?? throw new NullReferenceException("title")
            );

            bible.Abbreviation = Osis.GetAbbreviation(osis) ?? id;
            bible.Language = CultureInfo.GetCultureInfo(language);

            return bible;
        }

        protected override void ConfigureQuery(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field("bible")
                .Type<BibleType>()
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
}