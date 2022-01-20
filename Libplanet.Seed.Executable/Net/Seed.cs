#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Libplanet.Crypto;
using Libplanet.Net;
using Libplanet.Net.Protocols;
using Libplanet.Net.Transports;
using Serilog;

namespace Libplanet.Seed.Executable.Net
{
    public class Seed
    {
        private readonly PrivateKey _privateKey;
        private readonly KademliaProtocol _kademliaProtocol;
        private readonly ITransport _transport;

        public Seed(
            PrivateKey privateKey,
            string? host,
            int? port,
            int workers,
            IceServer[] iceServers,
            AppProtocolVersion appProtocolVersion,
            string transportType)
        {
            Table = new RoutingTable(privateKey.ToAddress());
            _privateKey = privateKey;
            switch (transportType)
            {
                case "tcp":
                    _transport = new TcpTransport(
                        Table,
                        privateKey,
                        appProtocolVersion,
                        null,
                        host: host,
                        listenPort: port,
                        iceServers: iceServers,
                        differentAppProtocolVersionEncountered: null,
                        minimumBroadcastTarget: 0);
                    break;
                case "netmq":
                    _transport = new NetMQTransport(
                        Table,
                        privateKey,
                        appProtocolVersion,
                        null,
                        workers: workers,
                        host: host,
                        listenPort: port,
                        iceServers: iceServers,
                        differentAppProtocolVersionEncountered: null,
                        minimumBroadcastTarget: 0);
                    break;
                default:
                    Log.Error(
                        "-t/--transport-type must be either \"tcp\" or \"netmq\".");
                    Environment.Exit(1);
                    return;
            }

            _kademliaProtocol = new KademliaProtocol(
                Table,
                _transport,
                privateKey.ToAddress());
        }

        public RoutingTable Table { get; }

        public async Task StartAsync(
            HashSet<BoundPeer> staticPeers,
            CancellationToken cancellationToken)
        {
            var tasks = new List<Task>
            {
                StartTransportAsync(cancellationToken),
                RefreshTableAsync(cancellationToken),
                RebuildConnectionAsync(cancellationToken),
            };
            if (staticPeers.Any())
            {
                tasks.Add(CheckStaticPeersAsync(staticPeers, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }

        public async Task StopAsync(TimeSpan waitFor)
        {
            await _transport.StopAsync(waitFor);
        }

        private async Task StartTransportAsync(CancellationToken cancellationToken)
        {
            await _transport.StartAsync(cancellationToken);
            Task task = _transport.StartAsync(cancellationToken);
            await _transport.WaitForRunningAsync();
            await task;
        }

        private async Task RefreshTableAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    await _kademliaProtocol.RefreshTableAsync(
                        TimeSpan.FromSeconds(60),
                        cancellationToken);
                    await _kademliaProtocol.CheckReplacementCacheAsync(cancellationToken);
                }
                catch (OperationCanceledException e)
                {
                    Log.Warning(e, $"{nameof(RefreshTableAsync)}() is cancelled.");
                    throw;
                }
                catch (Exception e)
                {
                    var msg = "Unexpected exception occurred during " +
                              $"{nameof(RefreshTableAsync)}(): {{0}}";
                    Log.Warning(e, msg, e);
                }
            }
        }

        private async Task RebuildConnectionAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
                    await _kademliaProtocol.RebuildConnectionAsync(
                        Kademlia.MaxDepth,
                        cancellationToken);
                }
                catch (OperationCanceledException e)
                {
                    Log.Warning(e, "{FName}() is cancelled.", nameof(RebuildConnectionAsync));
                    throw;
                }
                catch (Exception e)
                {
                    Log.Error(
                        e,
                        "Unexpected exception occurred during {FName}()",
                        nameof(RebuildConnectionAsync));
                }
            }
        }

        private async Task CheckStaticPeersAsync(
            IEnumerable<BoundPeer> peers,
            CancellationToken cancellationToken)
        {
            var boundPeers = peers as BoundPeer[] ?? peers.ToArray();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                    Log.Warning("Checking static peers. {@Peers}", boundPeers);
                    var peersToAdd = boundPeers.Where(peer => !Table.Contains(peer)).ToArray();
                    if (peersToAdd.Any())
                    {
                        Log.Warning("Some of peers are not in routing table. {@Peers}", peersToAdd);
                        await _kademliaProtocol.AddPeersAsync(
                            peersToAdd,
                            TimeSpan.FromSeconds(5),
                            cancellationToken);
                    }
                }
                catch (OperationCanceledException e)
                {
                    Log.Warning(e, $"{nameof(CheckStaticPeersAsync)}() is cancelled.");
                    throw;
                }
                catch (Exception e)
                {
                    var msg = "Unexpected exception occurred during " +
                              $"{nameof(CheckStaticPeersAsync)}(): {{0}}";
                    Log.Warning(e, msg, e);
                }
            }
        }
    }
}
