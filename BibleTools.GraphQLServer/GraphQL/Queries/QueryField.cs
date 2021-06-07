using HotChocolate.Types;

namespace BibleTools.GraphQLServer.GraphQL.Queries
{
    public abstract class QueryField : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            ConfigureQuery(descriptor);

            descriptor.Name(OperationTypeNames.Query);
        }

        protected abstract void ConfigureQuery(IObjectTypeDescriptor descriptor);
    }
}