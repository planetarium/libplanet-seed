using System;
using Libplanet.Net;

namespace Libplanet.Seed
{
    public struct PeerInfo
    {
        public BoundPeer BoundPeer;
        public DateTimeOffset LastUpdated;
        public TimeSpan? Latency;
    }
}
