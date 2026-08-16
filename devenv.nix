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

  # https://devenv.sh/reference/options/ (opencode.* options)
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

    ## Build, test, and docs

    - Run `dotnet build src` after C# changes and before finishing a task.
    - Run `dotnet test src` after C# changes to verify the tests pass on all target frameworks.
    - The library and its tests target net8.0, net9.0, net10.0, and net11.0; the devenv shell provides SDKs 8, 9, 10, and 11.
    - Tests run offline against a real Optimizely SDK built from an embedded datafile; do not add tests that require an SDK key or network access.
    - Docs are generated with docfx from `docs/docfx.json` (see the `docs` script); when adding a docs page, register it in `docs/docs/toc.yml`.
    - Use conventional commit messages.

    ## Using devenv

    - Enter the dev environment with `devenv shell`; all dotnet SDKs (8-11) and repo scripts are available there.
    - Repo scripts are defined under `scripts.*` in `devenv.nix` and run as `<name>` inside the shell (or `devenv <name>`): `restore`, `build`, `test`, `docs`.
    - Run `devenv test`, `devenv lint`, and `devenv check` to run the configured tests, git-hook linting, and CI checks.
    - `devenv.yaml` and `devenv.lock` pin the devenv inputs; after changing `devenv.yaml`, run `devenv update` to refresh the lock.
    - Generated files under `.opencode/` and `opencode.jsonc` are written from `devenv.nix`; never edit them by hand.

    ## OpenCode configuration

    - The source of truth for opencode is the `opencode.*` section of `devenv.nix`. The generated files (`.opencode/*`, `opencode.jsonc`) are gitignored; always edit `devenv.nix` instead.
    - `opencode.rules` is written to `.opencode/AGENTS.md` (these instructions).
    - `opencode.commands` is written to `.opencode/commands/<name>.md` (slash commands such as `/build`, `/test`, `/verify`, `/docs`).
    - `opencode.settings` is written to `opencode.jsonc`; `opencode.mcp` adds MCP servers to `opencode.jsonc` (values in `opencode.settings.mcp` take precedence).
    - `opencode.agents`, `opencode.skills`, `opencode.tools`, and `opencode.themes` write to their respective `.opencode/` subdirectories.
    - To update opencode configuration: edit `devenv.nix`, run `devenv shell` to regenerate the files, then restart opencode, since opencode loads its config once at startup and does not hot-reload it.
    - Validate opencode option shapes against https://opencode.ai/config.json before writing; opencode refuses to start on invalid config.
    - When working on opencode configuration, load the `customize-opencode` skill first.
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
      # Run the test suite on all target frameworks (net8.0, net9.0, net10.0, net11.0)

      ```bash
      dotnet test src
      ```
    '';
    verify = ''
      # Build and run the test suite

      ```bash
      dotnet build src && dotnet test src
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
    echo "FeatureManagement.Optimizely — use \`devenv shell\`; run \`build\`, \`test\`, \`verify\`, or \`docs\`"
  '';

  # https://devenv.sh/tests/
  enterTest = ''
    dotnet test src
    echo "Tests passed"
  '';
}
