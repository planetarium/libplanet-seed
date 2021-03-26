# Libplanet Seed

A bootstrapping node for peer discovery can be used with all libplanet-based nodes.

## Run

```
$ dotnet run --project ./Libplanet.Seed.Executable/Libplanet.Seed.Executable.csproj -- --help

Libplanet.Seed.Executable 1.0.0
Copyright (C) 2020 Libplanet.Seed.Executable

  -l, --log-level               (Default: information) Minimum severity for logging.
                                Should be one of error, warning, information, debug, verbose.
  -h, --host                    (Default: localhost) The host address to listen.
  -p, --port                    (Default: 5001) The port number to listen.
  -w, --workers                 (Default: 30) The number of concurrent message processing workers.
  -H, --graphql-host            (Default: localhost) The host address to listen graphql queries.
  -P, --graphql-port            (Default: 5000) The port number to listen graphql queries.
  -V, --app-protocol-version    Required. An app protocol version token.
  -k, --private-key             Private key used for node identifying and message signing.
  -I, --ice-server              URL to ICE server (TURN/STUN) to work around NAT.
  --peers                       A list of peers that must exist in the peer table.
                                The format of each peer is a comma-separated triple of a peer's
                                hexadecimal public key, host, and port number.
  --help                        Display this help screen.
  --version                     Display version information.
```

## Command Line Options

 - `-l`, `--log-level`: Minimum severity for logging. Should be one of error, warning, information, debug, verbose.
 - `-h`, `--host`: The host address to listen.
 - `-p`, `--port`: The port number to listen.
 - `-w`, `--workers`: The number of concurrent message processing workers.
 - `-H`, `--graphql-host`: The host address to listen graphql queries.
 - `-P`, `--graphql-port`: The port number to listen graphql queries.
 - `-V`, `--app-protocol-version`: An app protocol version token.
 - `-k`, `--private-key`: Private key used for node identifying and message signing.
 - `-I`, `--ice-server`: URL to ICE server (TURN/STUN) to work around NAT.
 - `--peers`: A list of peers that must exist in the peer table. The format of each peer is a comma-separated triple of a peer's hexadecimal public key, host, and port number. If omitted, peer checking is automatically disabled.

## Docker Build

A Standalone image can be created by running the command below in the directory where the solution is located.

```
$ docker build . -t <IMAGE_TAG>
```

## Related Projects

- [Libplanet](https://github.com/planetarium/libplanet)
- [NineChronicles.Standalone](https://github.com/planetarium/NineChronicles.Standalone)
