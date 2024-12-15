{
  description = "A Nix-flake-based .NET development environment";

  inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";

  outputs = {
    self,
    nixpkgs,
  }: let
    supportedSystems = ["x86_64-linux" "aarch64-linux" "x86_64-darwin" "aarch64-darwin"];
    forEachSupportedSystem = f:
      nixpkgs.lib.genAttrs supportedSystems (system:
        f {
          pkgs = import nixpkgs {
            inherit system;

            config.permittedInsecurePackages = [
              "dotnet-sdk-6.0.428"
            ];
          };
        });
  in {
    devShells = forEachSupportedSystem ({pkgs}: {
      default = let
        dotnet-combined = with pkgs.dotnetCorePackages; combinePackages [sdk_6_0 sdk_8_0 sdk_9_0];
      in
        pkgs.mkShell {
          DOTNET_ROOT = "${dotnet-combined}";
          packages = with pkgs; [dotnet-combined];
        };
    });
  };
}
