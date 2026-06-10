{
  description = "Chatbot development environment";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-parts.url = "github:hercules-ci/flake-parts";
    devshell.url = "github:numtide/devshell";
    treefmt-nix.url = "github:numtide/treefmt-nix";
  };

  outputs = inputs @ {
    self,
    nixpkgs,
    flake-parts,
    devshell,
    treefmt-nix,
  }:
    flake-parts.lib.mkFlake {inherit inputs;} {
      imports = [
        devshell.flakeModule
        treefmt-nix.flakeModule
      ];

      systems = ["x86_64-linux" "aarch64-linux" "x86_64-darwin" "aarch64-darwin"];

      perSystem = {
        config,
        pkgs,
        system,
        ...
      }: {
        treefmt = {
          projectRootFile = "flake.nix";
          programs.alejandra.enable = true;
          programs.prettier.enable = true;
          settings.global.excludes = [".agents/**" "**/openapi.json" "client/app/api-client/**"];
          settings.formatter.dotnet-format-whitespace = {
            command = "dotnet";
            options = ["format" "whitespace" "--include"];
            includes = ["*.cs"];
          };
          settings.formatter.dotnet-format-style = {
            command = "dotnet";
            options = ["format" "style" "--include"];
            includes = ["*.cs"];
          };
        };

        devshells.default = {
          name = "chatbot-dev";

          env = [
            {
              name = "BROWSER";
              value = "wsl-open";
            }
          ];

          packages = with pkgs; [
            bun
            cue
            dotnet-sdk_10
            just
            tilt
            docker
            docker-compose
            gitleaks
            lefthook
            openspec
            xdg-utils
            infisical
            config.treefmt.build.wrapper
          ];

          commands = [
            {
              name = "run";
              command = "just run";
              help = "Start the Tilt development environment";
            }
            {
              name = "setup";
              command = "just setup";
              help = "Full first-time setup: hooks → frontend → infra";
            }
          ];
        };
      };
    };
}
