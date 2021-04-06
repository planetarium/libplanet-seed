using System.Collections.Generic;
using GraphQL.Types;
using Libplanet.Net;
using Libplanet.Net.Protocols;
using Libplanet.Seed.GraphTypes;

namespace Libplanet.Seed.Queries
{
    public class Query : ObjectGraphType
    {
        private static RoutingTable _table;

        public Query(RoutingTable table)
        {
            _table = table;

            Field<NonNullGraphType<ListGraphType<NonNullGraphType<PeerStateType>>>>(
                "peers",
                resolve: _ => ListPeers
            );
        }

        internal static IEnumerable<PeerState> ListPeers => _table.PeerStates;
    }
}
