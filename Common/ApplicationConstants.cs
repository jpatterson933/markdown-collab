namespace MarkdownCollab.Common;

public static class ApplicationConstants
{
    public static class Authentication
    {
        public const string SessionKey = "Authenticated";
        public const string SessionValue = "true";
        public const string PasswordConfigKey = "SitePassword";
        public const int SessionIdleTimeoutHours = 24;
    }

    public static class Routes
    {
        public const string Index = "/Index";
        public const string Login = "/login";
        public const string Authenticate = "/authenticate";
        public const string Logout = "/logout";
    }

    public static class Diagrams
    {
        public const string DefaultContent = @"# Welcome to Collaborative Markdown Editor

This is a **real-time collaborative** markdown editor with *Mermaid diagram* support!

## Features
- Write in **markdown** syntax
- Collaborate with others in real-time
- Embed Mermaid diagrams

## Example Diagram

```mermaid
graph TD
    A[Start Editing] --> B[Write Markdown]
    B --> C[Add Diagrams]
    C --> D[Collaborate]
    D --> E[Share Your Work]
```

## Try it out!
Start editing this document and see changes sync across all connected users.";
        public const int MaxContentLength = 1_000_000;
    }

    public static class RoomCode
    {
        public const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public const int Length = 8;
        public const int MaxLength = 10;
    }

    public static class SignalR
    {
        public const string LoadDiagram = "LoadDiagram";
        public const string DiagramUpdated = "DiagramUpdated";
        public const string HubPath = "/diagramhub";
    }

    public static class Database
    {
        public const string DefaultConnectionKey = "DefaultConnection";
        public const string EnvironmentVariableName = "DATABASE_URL";
        public const string PostgresUrlPrefix = "postgres://";
        public const string InMemoryDatabaseName = "MarkdownCollab";
    }

    public static class ErrorMessages
    {
        public const string InvalidPassword = "Incorrect password. Please try again.";
        public const string PasswordNotConfigured = "SitePassword must be configured in appsettings.Development.json or as environment variable";
    }
}
