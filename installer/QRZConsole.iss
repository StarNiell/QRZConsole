; Inno Setup script for QRZ Console (https://jrsoftware.org/isinfo.php).
; Built by the GitHub "Release" workflow; to build it locally after a "dotnet publish":
;   iscc /DAppVersion=2.0.0-beta /DNumericVersion=2.0.0 installer\QRZConsole.iss

#ifndef AppVersion
  #define AppVersion "0.0.0-dev"
#endif
#ifndef NumericVersion
  #define NumericVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\publish\QRZConsole"
#endif
#ifndef OutputDir
  #define OutputDir "..\dist"
#endif

#define AppName "QRZ Console"
#define AppExe "QRZConsole.exe"
#define AppUrl "https://github.com/StarNiell/QRZConsole"
; Evergreen WebView2 Runtime bootstrapper (Microsoft)
#define WebView2Url "https://go.microsoft.com/fwlink/p/?LinkId=2124703"

[Setup]
; Never change AppId: it identifies the application for upgrades and uninstall.
AppId={{8F3A6C2E-4B7D-4E1A-9C5F-2D8E7B1A6F40}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=IU8NQI
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
AppUpdatesURL={#AppUrl}/releases
VersionInfoVersion={#NumericVersion}
VersionInfoProductTextVersion={#AppVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
; Per-user install by default (no administrator rights); the user can choose "all users".
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
OutputDir={#OutputDir}
OutputBaseFilename=QRZConsole-{#AppVersion}-win-x64-setup
SetupIconFile=..\QRZConsole\QRZ.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"

[CustomMessages]
english.WebView2Missing=QRZ Console needs the Microsoft Edge WebView2 Runtime, which was not found on this PC.%n%nOpen the Microsoft download page now?
italian.WebView2Missing=QRZ Console richiede Microsoft Edge WebView2 Runtime, che non risulta installato su questo PC.%n%nAprire ora la pagina di download di Microsoft?

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
const
  WebView2Key = 'Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';

function HasVersion(RootKey: Integer; SubKey: String): Boolean;
var
  Version: String;
begin
  Result := RegQueryStringValue(RootKey, SubKey, 'pv', Version) and (Version <> '') and (Version <> '0.0.0.0');
end;

{ WebView2 Runtime registration: machine-wide (32/64-bit view) or per user. }
function IsWebView2Installed: Boolean;
begin
  Result := HasVersion(HKLM, 'SOFTWARE\WOW6432Node\' + WebView2Key)
         or HasVersion(HKLM, 'SOFTWARE\' + WebView2Key)
         or HasVersion(HKCU, 'Software\' + WebView2Key);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ErrorCode: Integer;
begin
  if (CurStep = ssPostInstall) and not WizardSilent and not IsWebView2Installed then
  begin
    if MsgBox(CustomMessage('WebView2Missing'), mbConfirmation, MB_YESNO) = IDYES then
      ShellExec('open', '{#WebView2Url}', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
  end;
end;
