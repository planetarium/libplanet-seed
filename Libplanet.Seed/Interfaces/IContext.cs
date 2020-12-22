using System.Runtime.CompilerServices;
using GraphQL.Types;
using Libplanet.Net.Protocols;
using Libplanet.Seed.Queries;

namespace Libplanet.Seed.Interfaces
{
    public interface IContext
    {
        RoutingTable Table { get; }
    }

    public static class SeedContext
    {
        private static ConditionalWeakTable<object, Schema> _schemaObjects =
            new ConditionalWeakTable<object, Schema>();

        public static Schema GetSchema(this IContext context)
        {
            return _schemaObjects.GetValue(
                context,
                (_) =>
                {
                    var s = new Schema { Query = new Query(context.Table) };
                    return s;
                });
        }
    }
}
