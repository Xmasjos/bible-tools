
using HotChocolate.Types;

using BibleTools.GraphQLServer.Models;

namespace BibleTools.GraphQLServer.GraphQL.Types
{
    public class LanguageType : ObjectType<BibleLanguage>
    {
        protected override void Configure(IObjectTypeDescriptor<BibleLanguage> descriptor)
        {
            descriptor.Name("language");

            descriptor.Field(t => t.Language)
                .Name("language")
                .Type<StringType>()
                .Resolve<string?>(context => context.Parent<BibleLanguage>()?.Language?.EnglishName);

            descriptor.Field("languageIETF")
                .Type<StringType>()
                .Resolve<string?>(context => context.Parent<BibleLanguage>()?.Language?.IetfLanguageTag);

            descriptor.Field(t => t.Bibles)
                .Name("bibles")
                .Type<ListType<NonNullType<BibleType>>>()
                .Resolve(context => context.Parent<BibleLanguage>().Bibles); //TODO: Load if not available
        }
    }
}