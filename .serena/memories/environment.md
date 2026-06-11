# Local Environment & Tooling

## Reproducible Environment

- Controlled via Nix Flakes and Direnv.
- Enter manually with `nix develop` or automatically with `direnv allow`.
- Local stack orchestrated using Just (`just run` command).

## CLI & Shell Execution

- Prefix commands with the `snip --` proxy to optimize tokens.
- **Sandboxing Issues:** Standard sandbox execution can fail due to local Nix wrapper boundaries. If a command fails or requires local tooling context, configure with `BypassSandbox: true` and execute.
- Always run `just format` to format files before committing.
