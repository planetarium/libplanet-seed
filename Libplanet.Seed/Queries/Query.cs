using System.Collections.Concurrent;
using GraphQL.Types;
using Libplanet.Crypto;
using Libplanet.Seed.GraphTypes;

namespace Libplanet.Seed.Queries
{
    public class Query : ObjectGraphType
    {
        public Query(ConcurrentDictionary<Address, PeerInfo>? peers)
        {
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<PeerInfoType>>>>(
                "peers",
                resolve: _ => peers?.Values
            );
        }
    }
}
