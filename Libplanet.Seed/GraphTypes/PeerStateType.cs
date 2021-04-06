using System;
using GraphQL.Types;
using Libplanet.Net;

namespace Libplanet.Seed.GraphTypes
{
    public class PeerStateType : ObjectGraphType<PeerState>
    {
        public PeerStateType()
        {
            Field(
                name: "address",
                x => x.Peer.Address,
                type: typeof(NonNullGraphType<IdGraphType>));
            Field(
                name: "endPoint",
                x => x.Peer.EndPoint,
                type: typeof(NonNullGraphType<IdGraphType>));
            Field(
                name: "publicIPAddress",
                x => x.Peer.PublicIPAddress,
                nullable: true,
                type: typeof(IdGraphType));
            Field(
                name: "lastUpdated",
                x => x.LastUpdated,
                type: typeof(NonNullGraphType<IdGraphType>));
            Field(
                name: "lastChecked",
                x => x.LastChecked,
                nullable: true,
                type: typeof(IdGraphType));
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
