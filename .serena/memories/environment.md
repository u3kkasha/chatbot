# Local Environment & Tooling

## Reproducible Environment

- Controlled via Nix Flakes and Direnv.
- Enter manually with `nix develop` or automatically with `direnv allow`.
- Local stack orchestrated using Just (`just run` command).
- **Environment Initialization:** Use `just init-env` (cross-platform Node script) to set up `.env.local`. It verifies Infisical login status before copying.
- **Secret Precedence:** Secrets are loaded from Infisical first, then `.env.local` acts as an override layer (defined in `.envrc`).

## CLI & Shell Execution

- Prefix commands with the `snip --` proxy to optimize tokens.
- **Sandboxing Issues:** Standard sandbox execution can fail due to local Nix wrapper boundaries. If a command fails or requires local tooling context, configure with `BypassSandbox: true` and execute.
- Always run `just format` to format files before committing.
