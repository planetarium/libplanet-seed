using System.Collections.Concurrent;
using GraphQL.Types;
using Libplanet.Crypto;
using Libplanet.Seed.GraphTypes;

namespace Libplanet.Seed.Queries
{
    public class Query : ObjectGraphType
    {
        public Query(
            ConcurrentDictionary<Address, PeerInfo>? peers,
            ConcurrentDictionary<Address, PeerInfo>? gossipPeers)
        {
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<PeerInfoType>>>>(
                "peers",
                resolve: _ => peers?.Values
            );

            Field<NonNullGraphType<ListGraphType<NonNullGraphType<PeerInfoType>>>>(
                "gossipPeers",
                resolve: _ => gossipPeers?.Values
            );
        }
    }
}
