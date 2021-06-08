
using HotChocolate.Types;

using BibleTools.GraphQLServer.Models;

namespace BibleTools.GraphQLServer.GraphQL.Types
{
    public class BibleType : ObjectType<Bible>
    {
        protected override void Configure(IObjectTypeDescriptor<Bible> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Field(t => t.Title).Name("title").Type<NonNullType<StringType>>();
            descriptor.Field(t => t.BibleType).Name("type").Type<NonNullType<BibleTypeEnumType>>();
            descriptor.Field(t => t.Id).Name("id").Type<NonNullType<StringType>>();

            descriptor.Field(t => t.Abbreviation)
                .Name("abbreviation")
                .Type<NonNullType<StringType>>()
                .Resolve<string>(context =>
                {
                    var parent = context.Parent<Bible>();

                    return parent.Abbreviation ?? parent.Id; //TODO: Retrieve from file if not available
                });

            descriptor.Field(t => t.Language)
                .Name("language")
                .Type<StringType>()
                .Resolve<string?>(context => context.Parent<Bible>()?.Language?.EnglishName);

            descriptor.Field("languageIETF")
                .Type<StringType>()
                .Resolve<string?>(context => context.Parent<Bible>()?.Language?.IetfLanguageTag);
        }
    }
}