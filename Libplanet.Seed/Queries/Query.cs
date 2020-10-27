using System.Collections.Generic;
using GraphQL.Types;
using Libplanet.Net;
using Libplanet.Seed.GraphTypes;

namespace Libplanet.Seed.Queries
{
    public class Query : ObjectGraphType
    {
        private static NetMQTransport _transport;

        public Query(NetMQTransport transport)
        {
            _transport = transport;

            Field<NonNullGraphType<ListGraphType<NonNullGraphType<PeerType>>>>(
                "peers",
                resolve: _ => ListPeers
            );
        }

        internal static IEnumerable<BoundPeer> ListPeers => _transport.Peers;
    }
}
