{
  pkgs,
  lib,
  config,
  inputs,
  ...
}:
{
  # https://devenv.sh/basics/
  env.GREET = "devenv";

  # https://devenv.sh/packages/
  packages = [ pkgs.git ];

  # https://devenv.sh/languages/
  languages.dotnet.enable = true;
  languages.dotnet.package = pkgs.dotnetCorePackages.combinePackages [
    pkgs.dotnetCorePackages.sdk_8_0
    pkgs.dotnetCorePackages.sdk_9_0
    pkgs.dotnetCorePackages.sdk_10_0
    pkgs.dotnetCorePackages.sdk_11_0
  ];

  # https://devenv.sh/scripts/
  scripts.restore.exec = "dotnet restore src";
  scripts.build.exec = "dotnet build src";
  scripts.test.exec = "dotnet test src";
  scripts.docs.exec = "dotnet tool restore && dotnet tool run docfx docs/docfx.json";

  # https://devenv.sh/git-hooks/
  git-hooks.hooks.nixfmt.enable = true;

  # https://devenv.sh/integrations/opencode/
  opencode.enable = true;

  # Attributes written to opencode.jsonc
  opencode.settings = { };

  # devenv MCP server so opencode can manage the shell
  opencode.mcp.devenv = {
    type = "local";
    command = [
      "devenv"
      "mcp"
    ];
    environment = {
      DEVENV_ROOT = "{env:DEVENV_ROOT}";
    };
  };

  # Global instructions -> .opencode/AGENTS.md
  opencode.rules = ''
    # Development Rules

    - Run `dotnet build src` after C# changes and before finishing a task.
    - The library targets net8.0, net9.0, net10.0, and net11.0; the devenv shell provides SDKs 8, 9, 10, and 11.
    - Docs are generated with docfx from `docs/docfx.json` (see the `docs` script).
    - Use conventional commit messages.
  '';

  # Slash commands -> .opencode/commands/
  opencode.commands = {
    build = ''
      # Build the solution

      ```bash
      dotnet build src
      ```
    '';
    test = ''
      # Run tests

      ```bash
      dotnet test src
      ```
    '';
    docs = ''
      # Generate documentation with docfx

      ```bash
      dotnet tool restore && dotnet tool run docfx docs/docfx.json
      ```
    '';
  };

  # https://devenv.sh/basics/
  enterShell = ''
    echo "FeatureManagement.Optimizely — use \`devenv shell\`; run \`build\`, \`test\`, or \`docs\`"
  '';

  # https://devenv.sh/tests/
  enterTest = ''
    dotnet build src
    echo "Tests passed"
  '';
}
