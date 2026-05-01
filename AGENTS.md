# Scoreboard Agents

## Build
```bash
dotnet build Scoreboard/Scoreboard.csproj
```

## Settings Persistence
Settings are in `Properties/Settings.settings` and `Properties/Settings.Designer.cs`. To add a new setting:
1. Add to both files
2. Load in `Score.cs` constructor: `Property = Properties.Settings.Default.Property`
3. Save in property setter: `Properties.Settings.Default.Property = value; Properties.Settings.Default.Save();`

## SwappedGame Pattern
When creating a `SwappedGame`, pass `this` (the Score reference):
```csharp
new SwappedGame(_currentGame, _secondarySwapped, this)
```
The `Score` reference is needed because card collections (`Team1Cards`, `Team2Cards`) live on `Score`, not `Game`.

## WPF Property Change Notifications
`SwappedGame` wraps a `Game` and must forward property changes with swapped names:
- Team1 → Team2, Team2 → Team1 (and scores, colors, flags)
- When `_swapped = false`, forward unchanged property names
- Subscribe to `Game.PropertyChanged` and `Score.Team1Cards.ListChanged`/`Team2Cards.ListChanged`

## Architecture
- **Scoreboard**: WPF app (WinExe, net8.0-windows)
- **Utilities**: Shared library
- **Testing**: Python server test only
- Secondary displays: XAML window (WPF bindings), HTML (web sockets via ScoreboardServer), LED (ProtoSlave web sockets)

## Style
- Use `new(...)` shorthand when type is apparent (per .editorconfig)
- No comments unless user explicitly requests them