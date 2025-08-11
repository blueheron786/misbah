# Copilot Instructions for Misbah

## Project Overview
- **Misbah** is a clean-architecture, cross-platform note-taking app inspired by Obsidian, written in C#/.NET 8.
- The solution is split into multiple projects by responsibility:
  - `Misbah.BlazorShared`: Shared Blazor components and core UI logic (Razor Class Library, testable, no platform dependencies)
  - `Misbah.BlazorDesktop`: WPF/Blazor Hybrid desktop app (entry point, platform-specific wrappers)
  - `Misbah.Infrastructure`: Persistence, external services (disk, Git, search, etc.)
  - `Misbah.Application`: Application services, DTOs, interfaces
  - `Misbah.Core`: Markdown rendering and related logic
  - `Misbah.Domain`: Domain entities and interfaces
  - Test projects: Named as `<Project>.Tests` (e.g., `Misbah.Core.Tests`)

## Key Patterns & Conventions
- **Clean Architecture**: Each layer references only the layer(s) below it. UI never references Infrastructure directly.
- **Blazor Components**: All testable/shared Blazor components live in `Misbah.BlazorShared`. Platform-specific code (e.g., WPF interop) stays in `Misbah.BlazorDesktop`.
- **Testing**: Use xUnit and bUnit for Blazor component/unit tests. Test projects reference only the shared/testable libraries, not platform-specific ones.
- **Resource Access**: Shared resources (e.g., `AppStrings`) are accessed via static properties in `Misbah.BlazorShared.Resources`.
- **DTOs**: All cross-layer data transfer objects are in `Misbah.Application.DTOs`.
- **External Integrations**: All file system, Git, and other integrations are abstracted in `Misbah.Infrastructure` and injected via DI.

## Developer Workflows
- **Build**: `dotnet build Misbah.sln` or use the VS Code build task.
- **Run Desktop App**: `dotnet run --project Misbah.BlazorDesktop`
- **Test**: `dotnet test` (runs all tests), or `dotnet test ./Misbah.Core.Tests/Misbah.Core.Tests.csproj` for a specific project.
- **NuGet Issues**: If you see missing DLLs (e.g., AngleSharp), clear cache and rebuild:
  - `dotnet nuget locals all --clear`
  - Delete `bin/` and `obj/` in the affected project
  - `dotnet restore && dotnet build && dotnet test`

## Special Notes
- **Do not reference desktop-only types in shared/test projects**. If you need to test UI logic, move it to `Misbah.BlazorShared`.
- **Obsidian Vault Compatibility**: The app can open and operate on Obsidian vaults directly (see onboarding logic in `Misbah.BlazorDesktop`).
- **Custom Markdown**: See `Misbah.Core` for custom syntax extensions (highlight, checkboxes, wiki-links).

## Examples
- To add a new Blazor component for both desktop and test, place it in `Misbah.BlazorShared/Components/` and reference it from both `Misbah.BlazorDesktop` and test projects.
- To add a new persistence provider, implement the relevant interface in `Misbah.Infrastructure` and register it in DI.

## Key Files/Dirs
- `Misbah.BlazorShared/Resources/AppStrings.cs`: Shared UI strings
- `Misbah.Application/DTOs/`: Data transfer objects
- `Misbah.Core/`: Markdown rendering logic
- `Misbah.Infrastructure/Services/`: External service implementations
- `Misbah.BlazorDesktop/`: WPF/Blazor entry point

---
If you are unsure about a pattern, check the README or look for similar examples in the relevant project. When in doubt, prefer testability and separation of concerns.
