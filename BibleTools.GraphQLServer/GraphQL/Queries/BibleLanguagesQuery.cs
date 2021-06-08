using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using HotChocolate.Types;

using BibleTools.GraphQLServer.GraphQL.Types;
using BibleTools.GraphQLServer.Models;

namespace BibleTools.GraphQLServer.GraphQL.Queries
{
    public class BibleLanguagesQuery : QueryField
    {
        protected override void ConfigureQuery(IObjectTypeDescriptor descriptor)
        {
            descriptor.Field("languages")
                .Type<ListType<NonNullType<LanguageType>>>()
                .Argument("type", argument => argument.Type<NonNullType<BibleTypeEnumType>>())
                // .Argument("language", argument => argument.Type<StringType>())
                .Resolve<IEnumerable<BibleLanguage>?>(async context =>
                {
                    var type = context.ArgumentValue<BibleTypeEnum>("type");

                    switch (type)
                    {
                        case BibleTypeEnum.Osis:
                            var osisBiblesByLanguage = await Osis.GetBibleNamesByLanguage(cancellationToken: context.RequestAborted);

                            return osisBiblesByLanguage.Select(t => new BibleLanguage(
                                language: CultureInfo.GetCultureInfo(t.Key),
                                bibles: t.Value.Select(q => new Bible(BibleTypeEnum.Osis, q.Id, q.Title)).ToList()
                            ));
                        // case BibleTypeEnum.Zefania:
                        default:
                            throw new NotImplementedException($"type '${type}'");
                    }
                });
        }
    }
}