# Clean Architecture Refactoring Plan

## Notes
- Refactor Misbah application to follow Clean Architecture principles
- Layers:
  1. Domain Layer: Core business logic and entities (Note, FolderNode)
  2. Application Layer: Use cases and interfaces (INoteService, IAutoSaveService, etc.)
  3. Infrastructure Layer: Implementations of interfaces (NoteService, AutoSaveService, etc.)
  4. Presentation Layer: UI components (Blazor components)
- Run tests after every change to ensure nothing is broken

## Task List
- [ ] Reorganize project structure into Clean Architecture layers
- [ ] Define core domain models in Domain layer
- [ ] Move interfaces to Application layer
- [ ] Move service implementations to Infrastructure layer
- [ ] Update Presentation layer to use interfaces from Application layer
- [ ] Run all tests after each refactor step

## Current Goal
Analyze and reorganize project structure into layers