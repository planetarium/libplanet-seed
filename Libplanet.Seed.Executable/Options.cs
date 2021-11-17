using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CommandLine;
using Libplanet.Crypto;
using Libplanet.Net;
using Serilog;

namespace Libplanet.Seed.Executable
{
    public class Options
    {
        [Option(
            'l',
            "log-level",
            Default = "information",
            HelpText = "Minimum severity for logging. " +
                       "Should be one of error, warning, information, debug, verbose.")]
        public string LogLevel { get; set; }

        [Option(
            'h',
            "host",
            Default = null,
            HelpText = "The host address to listen.")]
        public string Host { get; set; }

        [Option(
            'p',
            "port",
            Default = null,
            HelpText = "The port number to listen.")]
        public int? Port { get; set; }

        [Option(
            'w',
            "workers",
            Default = 30,
            HelpText = "The number of concurrent message processing workers. " +
                "Ignored if transport type is set to \"tcp\".")]
        public int Workers { get; set; }

        [Option(
            'H',
            "graphql-host",
            Default = "localhost",
            HelpText = "The host address to listen graphql queries.")]
        public string GraphQLHost { get; set; }

        [Option(
            'P',
            "graphql-port",
            Default = 5000,
            HelpText = "The port number to listen graphql queries.")]
        public int GraphQLPort { get; set; }

        [Option(
            'M',
            "minimum-broadcast-target",
            Default = 10,
            HelpText = "The number of minimum targets to broadcast.")]
        public int MinimumBroadcastTarget { get; set; }

        [Option(
            'V',
            "app-protocol-version",
            HelpText = "An app protocol version token.",
            Required = true)]
        public string AppProtocolVersionToken { get; set; }

        [Option(
            't',
            "transport-type",
            Default = "tcp",
            HelpText = "The type of transport to use. Should be either \"tcp\" or \"netmq\".")]
        public string TransportType { get; set; }

        [Option(
            'k',
            "private-key",
            HelpText = "Private key used for node identifying and message signing.")]
        public string PrivateKeyString
        {
            get => PrivateKey is null ? string.Empty : PrivateKey.ToString();

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

        public PrivateKey PrivateKey { get; set; }

        [Option(
            'I',
            "ice-server",
            HelpText = "URL to ICE server (TURN/STUN) to work around NAT.")]
        public string IceServerUrl
        {
            get
            {
                if (IceServer is null)
                {
                    return null;
                }

                Uri uri = IceServer.Urls.First();
                var uriBuilder = new UriBuilder(uri)
                {
                    UserName = IceServer.Username,
                    Password = IceServer.Credential,
                };
                return uriBuilder.Uri.ToString();
            }

            set
            {
                if (value is null)
                {
                    IceServer = null;
                    return;
                }

                var uri = new Uri(value);
                string[] userInfo = uri.UserInfo.Split(':', count: 2);
                IceServer = new IceServer(new[] { uri }, userInfo[0], userInfo[1]);
            }
        }

        public IceServer IceServer { get; set; }

        [Option(
            longName: "peers",
            HelpText = "A list of peers that must exist in the peer table. " +
                       "The format of each peer is a comma-separated triple of a peer's " +
                       "hexadecimal public key, host, and port number.")]
        public IEnumerable<string> PeerStrings
        {
            get
            {
                return Peers?.Select(peer => peer.ToString());
            }

            set
            {
                if (value is null)
                {
                    Peers = null;
                    return;
                }

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

        public IEnumerable<BoundPeer> Peers { get; set; }

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
            else if (result is NotParsed<Options> notParsed)
            {
                System.Environment.Exit(
                    notParsed.Errors.All(e => e.Tag is ErrorType.HelpRequestedError) ? 0 : 1
                );
            }

            return null;
        }
    }
}
