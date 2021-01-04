using System;
using System.Threading;
using System.Threading.Tasks;
using Libplanet.Crypto;
using Libplanet.Net;
using Libplanet.Net.Protocols;
using Libplanet.Seed.Executable.Exceptions;
using Libplanet.Seed.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace Libplanet.Seed.Executable
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Options options = Options.Parse(args, Console.Error);

            var loggerConfig = new LoggerConfiguration();
            loggerConfig = options.Debug
                ? loggerConfig.MinimumLevel.Debug()
                : loggerConfig.MinimumLevel.Information();
            loggerConfig = loggerConfig
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console();
            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                var privateKey = options.PrivateKey ?? new PrivateKey();

                RoutingTable table = new RoutingTable(privateKey.ToAddress());
                NetMQTransport transport = new NetMQTransport(
                    table,
                    privateKey,
                    AppProtocolVersion.FromToken(options.AppProtocolVersionToken),
                    null,
                    options.Workers,
                    options.Host,
                    options.Port,
                    new[] { options.IceServer },
                    null);
                KademliaProtocol peerDiscovery = new KademliaProtocol(
                    table,
                    transport,
                    privateKey.ToAddress());
                Startup.TableSingleton = table;

                IWebHost webHost = WebHost.CreateDefaultBuilder()
                    .UseStartup<SeedStartup<Startup>>()
                    .UseSerilog()
                    .UseUrls($"http://{options.GraphQLHost}:{options.GraphQLPort}/")
                    .Build();

                using (var cts = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        eventArgs.Cancel = true;
                        cts.Cancel();
                    };

                    try
                    {
                        await Task.WhenAll(
                            webHost.RunAsync(cts.Token),
                            StartTransportAsync(transport, cts.Token),
                            RefreshTableAsync(peerDiscovery, cts.Token),
                            RebuildConnectionAsync(peerDiscovery, cts.Token)
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        await transport.StopAsync(TimeSpan.FromSeconds(1));
                    }
                }
            }
            catch (InvalidOptionValueException e)
            {
                string expectedValues = string.Join(", ", e.ExpectedValues);
                Console.Error.WriteLine($"Unexpected value given through '{e.OptionName}'\n"
                                        + $"  given value: {e.OptionValue}\n"
                                        + $"  expected values: {expectedValues}");
            }
        }

        private static async Task StartTransportAsync(
            NetMQTransport transport,
            CancellationToken cancellationToken)
        {
            await transport.StartAsync(cancellationToken);
            Task task = transport.RunAsync(cancellationToken);
            await transport.WaitForRunningAsync();
            await task;
        }

        private static async Task RefreshTableAsync(
            KademliaProtocol protocol,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    await protocol.RefreshTableAsync(TimeSpan.FromSeconds(10), cancellationToken);
                    await protocol.CheckReplacementCacheAsync(cancellationToken);
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

        private static async Task RebuildConnectionAsync(
            KademliaProtocol protocol,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
                    await protocol.RebuildConnectionAsync(cancellationToken);
                }
                catch (OperationCanceledException e)
                {
                    Log.Warning(e, $"{nameof(RebuildConnectionAsync)}() is cancelled.");
                    throw;
                }
                catch (Exception e)
                {
                    var msg = "Unexpected exception occurred during " +
                              $"{nameof(RebuildConnectionAsync)}(): {{0}}";
                    Log.Warning(e, msg, e);
                }
            }
        }

        private class Startup : IContext
        {
            public RoutingTable Table => TableSingleton;

            internal static RoutingTable TableSingleton { get; set; }
        }
    }
}
