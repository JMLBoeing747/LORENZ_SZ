; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "LORENZ"
#define MyAppVersion "3.0.0"
#define MyAppVersionFull MyAppVersion + ""
#define MyAppPublisher "LORENZ SZ"
#define MyAppExeName "LORENZ.EXE"
#define AppPublishPath "..\LSZ\LORENZ\bin\Release\publish"
#define LZHELPPath "..\LZHELP\LZHELP.CHM"
#define CRYPTOPath "..\LSZ\CRYPTO\bin\Release\publish"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{172D101C-DFA8-42F2-A85B-5A39002B093E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersionFull}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\LORENZSZ
DisableDirPage=no
DefaultGroupName="LORENZ SZ"
DisableProgramGroupPage=yes
LicenseFile=EULA.TXT
InfoBeforeFile=BEFORE.TXT
InfoAfterFile=AFTER.TXT

; Special attributes
ArchitecturesAllowed=x86 x64
CloseApplications=yes

; Setup caracteristics
; Uncomment the following line to run in non administrative install mode (install for current user only.)
VersionInfoVersion={#MyAppVersion}
PrivilegesRequired=lowest
OutputDir=.
OutputBaseFilename=LORENZ_v{#MyAppVersionFull}
Compression=lzma
SolidCompression=yes
SetupIconFile="LZSETUP.ICO"
UninstallDisplayIcon="{app}\LORENZ.EXE"
WizardImageAlphaFormat=premultiplied
WizardStyle=modern
WizardSmallImageFile="LZICON.BMP"

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
Name: "crypto"; Description: "Installer CRYPTO si vous n'avez pas de cl� de produit"; Flags: unchecked

[Files]
Source: "{#LZHELPPath}"; DestDir:"{app}"; Flags: ignoreversion
Source: "{#AppPublishPath}\LORENZ.EXE"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppPublishPath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "{#CRYPTOPath}\*"; DestDir: "{app}\CRYPTO"; Tasks: crypto; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKA; Subkey:"Software\Classes\.LZI"; ValueType: string; ValueData: "LORENZ Initializer file"; Flags: createvalueifdoesntexist uninsdeletekey
Root: HKA; Subkey:"Software\Classes\.LKI"; ValueType: string; ValueData: "LORENZ Product key file"; Flags: createvalueifdoesntexist uninsdeletekey
Root: HKA; Subkey:"Software\Classes\.LC2"; ValueType: string; ValueData: "LORENZ Cipher File V2"; Flags: createvalueifdoesntexist uninsdeletekey
Root: HKCU; Subkey:"Environment"; ValueType: expandsz; ValueName: "LORENZPATH"; ValueData: "{app}"; Flags: uninsdeletevalue

[Run]
Filename: "{commonpf32}\LORENZ Schl�sselzusatz\unins000.exe"; StatusMsg: "Check for LORENZ uninstaller version 1.1.1"; Flags: skipifdoesntexist
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent 

[InstallDelete]
Type: files; Name: "{app}\CRYPTO\*"; Tasks: crypto
Type: dirifempty; Name: "{app}\CRYPTO"; Tasks: crypto
Type: files; Name: "{localappdata}\Programs\LORENZSZ\*" 

[UninstallDelete]
Type: dirifempty; Name: "{app}\LZPARAMS"