; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

; Press Shift + Alt + B to build in VS Code

#define MyAppName "Tourney Scoreboard"
#define MyAppVersion "1.30.004"
#define MyAppFileVersion "1_30_004"
#define MyAppPublisher "verner software"
#define MyAppURL "https://www.verner.co.nz"
#define MyAppExeName "Scoreboard.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{5B2314DC-E5A6-49DA-8DD8-B078C59810FD}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=TourneyScoreboardSetup_{#MyAppfILEVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\bin\Release\net5.0-windows\Scoreboard.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\Scoreboard.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\Scoreboard.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Properties\app.manifest"; DestDir: "{app}"; DestName: "Scoreboard.exe.manifest"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\Scoreboard.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\Utilities.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\Utilities.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\ref\*"; DestDir: "{app}\ref"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\bin\Release\net5.0-windows\runtimes\*"; DestDir: "{app}\runtimes\"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\bin\Release\net5.0-windows\sounds\*"; DestDir: "{app}\sounds"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\bin\Release\net5.0-windows\pages\*"; DestDir: "{app}\pages"; Flags: ignoreversion recursesubdirs createallsubdirs

Source: "..\bin\Release\net5.0-windows\client_secrets.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\Google.Apis.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\Newtonsoft.Json.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\System.Net.Http.Formatting*"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\System.DirectoryServices*"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net5.0-windows\Scoreboard.runtimeconfig*"; DestDir: "{app}"; Flags: ignoreversion
;Source: "..\bin\Release\net5.0-windows\System.ValueTuple.*"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, "&", "&&")}}"; Flags: nowait postinstall skipifsilent

