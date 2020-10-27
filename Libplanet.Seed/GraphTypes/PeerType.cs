using GraphQL.Types;
using Libplanet.Net;

namespace Libplanet.Seed.GraphTypes
{
    public class PeerType : ObjectGraphType<BoundPeer>
    {
        public PeerType()
        {
            Field(
                x => x.Address,
                type: typeof(NonNullGraphType<IdGraphType>));
            Field(
                x => x.EndPoint,
                type: typeof(NonNullGraphType<IdGraphType>));

            // This field may be null.
            Field(
                x => x.PublicIPAddress,
                nullable: true,
                type: typeof(IdGraphType));

            Name = "Peer";
        }
    }
}
