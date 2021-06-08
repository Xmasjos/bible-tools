
using HotChocolate.Types;

using BibleTools.GraphQLServer.Models;

namespace BibleTools.GraphQLServer.GraphQL.Types
{
    public class BibleTypeEnumType : EnumType<BibleTypeEnum>
    {
        protected override void Configure(IEnumTypeDescriptor<BibleTypeEnum> descriptor)
        {
            descriptor.Name("BibleType");

            descriptor.BindValuesImplicitly();
        }
    }
}