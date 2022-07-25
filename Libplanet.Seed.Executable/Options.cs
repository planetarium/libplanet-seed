using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CommandLine;
using Libplanet.Crypto;
using Libplanet.Net;

namespace Libplanet.Seed.Executable
{
    public class Options
    {
        [Option(
            'l',
            "log-level",
            Required = false,
            Default = "information",
            HelpText = "Minimum severity for logging. " +
                       "Should be one of error, warning, information, debug, verbose.")]
        public string? LogLevel { get; set; }

        [Option(
            'h',
            "host",
            Required = false,
            Default = null,
            HelpText = "The host address to listen.")]
        public string? Host { get; set; }

        [Option(
            'p',
            "port",
            Required = false,
            Default = null,
            HelpText = "The port number to listen.")]
        public int? Port { get; set; }

        [Option(
            'w',
            "workers",
            Required = false,
            Default = 30,
            HelpText = "The number of concurrent message processing workers. " +
                "Ignored if transport type is set to \"tcp\".")]
        public int Workers { get; set; }

        [Option(
            'H',
            "graphql-host",
            Required = false,
            Default = "localhost",
            HelpText = "The host address to listen graphql queries.")]
        public string? GraphQLHost { get; set; }

        [Option(
            'P',
            "graphql-port",
            Required = false,
            Default = 5000,
            HelpText = "The port number to listen graphql queries.")]
        public int GraphQLPort { get; set; }

        [Option(
            'V',
            "app-protocol-version",
            Required = true,
            HelpText = "An app protocol version token.")]
        public string? AppProtocolVersionToken { get; set; }

        [Option(
            'k',
            "private-key",
            Required = true,
            HelpText = "Private key used for node identifying and message signing.")]
        public string PrivateKeyString
        {
            set
            {
                PrivateKey = null;
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        PrivateKey = new PrivateKey(ByteUtil.ParseHex(value));
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(
                            "Error occurred during setting private key: {0};\n{1}",
                            value,
                            e);
                    }
                }
            }
        }

        public PrivateKey? PrivateKey { get; set; }

        [Option(
            'I',
            "ice-server",
            Required = false,
            Default = "",
            HelpText = "URL to ICE server (TURN/STUN) to work around NAT.")]
        public string IceServerUrl
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    IceServer = null;
                    return;
                }

                var uri = new Uri(value);
                string[] userInfo = uri.UserInfo.Split(':', count: 2);
                IceServer = new IceServer(uri);
            }
        }

        public IceServer? IceServer { get; set; }

        [Option(
            longName: "peers",
            Required = false,
            Default = new string[] { },
            HelpText = "A list of peers that must exist in the peer table. " +
                       "The format of each peer is a comma-separated triple of a peer's " +
                       "hexadecimal public key, host, and port number.")]
        public IEnumerable<string> PeerStrings
        {
            get
            {
                return Peers.Select(peer => peer.ToString());
            }

            set
            {
                Peers = value.Select(str =>
                {
                    string[] parts = str.Split(',');
                    if (parts.Length != 3)
                    {
                        throw new FormatException(
                            $"A peer must be a command-separated triple. {str}");
                    }

                    byte[] publicKeyBytes = ByteUtil.ParseHex(parts[0]);
                    var publicKey = new PublicKey(publicKeyBytes);
                    var endpoint = new DnsEndPoint(parts[1], int.Parse(parts[2]));
                    return new BoundPeer(publicKey, endpoint);
                });
            }
        }

        public IEnumerable<BoundPeer> Peers { get; private set; } = new BoundPeer[] { };

        [Option(
            longName: "maximum-peers-to-refresh",
            Required = false,
            Default = int.MaxValue,
            HelpText = "Maximum number of peers to be refreshed at once " +
                       "in periodic peer table refreshing task.")]
        public int MaximumPeersToRefresh { get; set; }

        [Option(
            longName: "refresh-interval",
            Required = false,
            Default = 5,
            HelpText = "Period in second of the peer table refreshing task.")]
        public int RefreshInterval { get; set; }

        [Option(
            longName: "peer-lifetime",
            Required = false,
            Default = 120,
            HelpText = "Lifespan by second determining whether " +
                       "a peer is stale and needs refreshing.")]
        public int PeerLifetime { get; set; }

        [Option(
            longName: "ping-timeout",
            Required = false,
            Default = 5,
            HelpText = "Timeout by second of reply to the pong message.")]
        public int PingTimeout { get; set; }

        public static Options Parse(string[] args, TextWriter errorWriter)
        {
            var parser = new Parser(with =>
            {
                with.AutoHelp = true;
                with.EnableDashDash = true;
                with.HelpWriter = errorWriter;
            });
            ParserResult<Options> result = parser.ParseArguments<Options>(args);

            if (result is Parsed<Options> parsed)
            {
                Options options = parsed.Value;
                return options;
            }

            if (result is NotParsed<Options> notParsed)
            {
                System.Environment.Exit(
                    notParsed.Errors.All(e => e.Tag is ErrorType.HelpRequestedError) ? 0 : 1
                );
            }

            throw new ArgumentException(
                "Unexpected error occurred parsing arguments.",
                nameof(args));
        }
    }
}
