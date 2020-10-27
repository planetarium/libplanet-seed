using System.Runtime.CompilerServices;
using GraphQL.Types;
using Libplanet.Net;
using Libplanet.Seed.Queries;

namespace Libplanet.Seed.Interfaces
{
    public interface ITransportContext
    {
        NetMQTransport Transport { get; }
    }

    public static class TransportContext
    {
        private static ConditionalWeakTable<object, Schema> _schemaObjects =
            new ConditionalWeakTable<object, Schema>();

        public static Schema GetSchema(this ITransportContext context)
        {
            return _schemaObjects.GetValue(
                context,
                (_) =>
                {
                    var s = new Schema { Query = new Query(context.Transport) };
                    return s;
                });
        }
    }
}