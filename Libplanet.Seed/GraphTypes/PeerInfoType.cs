using System;
using GraphQL.Types;
using Libplanet.Seed.Interfaces;

namespace Libplanet.Seed.GraphTypes
{
    public class PeerInfoType : ObjectGraphType<PeerInfo>
    {
        public PeerInfoType()
        {
            Field(
                name: "address",
                x => x.BoundPeer.Address,
                type: typeof(NonNullGraphType<IdGraphType>));
            Field(
                name: "endPoint",
                x => x.BoundPeer.EndPoint,
                type: typeof(NonNullGraphType<IdGraphType>));
            Field(
                name: "publicIPAddress",
                x => x.BoundPeer.PublicIPAddress,
                nullable: true,
                type: typeof(IdGraphType));
            Field(
                name: "lastUpdated",
                x => x.LastUpdated,
                type: typeof(NonNullGraphType<IdGraphType>));
            Field(
                name: "latency",
                x => x.Latency,
                nullable: true,
                type: typeof(IdGraphType));
            Field(
                name: "stale",
                x => (DateTimeOffset.UtcNow - x.LastUpdated),
                type: typeof(NonNullGraphType<IdGraphType>));

            Name = "PeerState";
        }
    }
}
