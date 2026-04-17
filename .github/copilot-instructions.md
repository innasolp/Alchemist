# Copilot Instructions

## Project Guidelines
- Do not use repository interfaces in code or tests; prefer using command/query handlers or DbContext directly (repositories are obsolete).
- In tests, use Moq to mock `IRequestHandler`/`ICommandHandler` handlers and set up their `Handle(...)` to call EF Core `DbContext` (InMemory) methods; do not create concrete handler classes.
- AlchemyContext cannot be constructed with DbContextOptions in tests; use a test-only derived context that overrides OnConfiguring to configure the EF InMemory provider (construct via parameterless constructor).