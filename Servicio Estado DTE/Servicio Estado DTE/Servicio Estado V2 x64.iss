; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{237BBFAF-C2CC-45F1-8397-F85ACCB02876}
AppName=Servico Estado FE
AppVersion=1.0.03
;AppVerName=Servicio Estado FE 1.0.02
AppPublisher=VisualD
AppPublisherURL=http://www.visuald.cl/
AppSupportURL=http://www.visuald.cl/
AppUpdatesURL=http://www.visuald.cl/
DefaultDirName={pf}\VisualD\Servicio Estado FE
DisableDirPage=yes
DefaultGroupName=Servico Estado FE
DisableProgramGroupPage=yes
OutputDir=Servicio Estado DTE\Servicio Estado DTE\Output
OutputBaseFilename=SetupServFE
Compression=lzma
SolidCompression=yes

[Languages]
Name: spanish; MessagesFile: compiler:Languages\Spanish.isl

[Files]
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\x64\Debug\Servicio Estado DTE.exe; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\Debug\App.config; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\Debug\Config.txt; DestDir: {app}
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\Debug\desinstalar servicio.bat; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\Debug\instalar servicio.bat; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\x64\Debug\Interop.SAPbobsCOM.dll; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\x64\Debug\Newtonsoft.Json.dll; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\Debug\SAPbobsCOM90.dll; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\x64\Debug\ServiceStack.Text.dll; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\x64\Debug\Servicio Estado DTE.exe.config; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\x64\Debug\Servicio Estado DTE.pdb; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\Debug\Datos.exe; DestDir: {app}; Flags: ignoreversion
Source: C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Servicio Estado DTE\Servicio Estado DTE\bin\Debug\Datos.pdb; DestDir: {app}; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\Servicio Estado FE; Filename: {app}\Servicio Estado DTE.exe

[Run]
Filename: {app}\Servicio Estado DTE.exe; Description: {cm:LaunchProgram,Servico Estado FE}; Flags: nowait postinstall skipifsilent
