; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Scoreboard"
#define MyAppVersion "1.9.004"
#define MyAppPublisher "verner software"
#define MyAppURL "http://www.verner.co.nz"
#define MyAppExeName "Scoreboard.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{DB573D8C-781F-445F-8E83-725B9C083658}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=ScoreboardSetup_1_9_004
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\Scoreboard.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\Scoreboard.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\Properties\app.manifest"; DestDir: "{app}"; DestName: "Scoreboard.exe.manifest"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\Scoreboard.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\Utilities.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\Utilities.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\sounds\*"; DestDir: "{app}\sounds"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\pages\*"; DestDir: "{app}\pages"; Flags: ignoreversion recursesubdirs createallsubdirs

Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\client_secrets.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\Google.Apis.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\Newtonsoft.Json.*"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\System.Net.Http.Formatting*"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Develop\verner software\Scoreboard\Scoreboard\bin\Release\System.ValueTuple.*"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, "&", "&&")}}"; Flags: nowait postinstall skipifsilent

