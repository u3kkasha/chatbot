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
          settings.global.excludes = [".agents/**"];
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
            dotnet-sdk_10
            tilt
            docker
            docker-compose
            gitleaks
            openspec
            xdg-utils
            config.treefmt.build.wrapper
          ];

          commands = [
            {
              name = "start";
              command = "tilt up";
              help = "Start the Tilt development environment";
            }
          ];
        };
      };
    };
}
