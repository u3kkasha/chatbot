{
  description = "Chatbot development environment";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-parts.url = "github:hercules-ci/flake-parts";
    devshell.url = "github:numtide/devshell";
  };

  outputs = inputs @ {
    self,
    nixpkgs,
    flake-parts,
    devshell,
    ...
  }:
    flake-parts.lib.mkFlake {inherit inputs;} {
      imports = [
        devshell.flakeModule
      ];

      systems = ["x86_64-linux" "aarch64-linux" "x86_64-darwin" "aarch64-darwin"];

      perSystem = {pkgs, ...}: {
        devshells.default = {
          name = "chatbot-dev";

          env = [
            {
              name = "BROWSER";
              value = "wsl-open";
            }
          ];

          packages = with pkgs; [
            alejandra
            bun
            cue
            dotnet-sdk_10
            nodejs_22
            just
            docker
            docker-compose
            gitleaks
            lefthook
            openspec
            xdg-utils
            infisical
            prettier
          ];

          commands = [
            {
              name = "setup";
              command = "just setup";
              help = "Install dependencies and start infrastructure";
            }
            {
              name = "run";
              command = "just run";
              help = "Start the development environment";
            }
          ];
        };
      };
    };
}
