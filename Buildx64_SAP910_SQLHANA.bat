IF EXIST GanttAXVD.ocx  (DEL GanttAXVD.ocx)
COPY  "GanttAXx86\GanttAXVD.ocx" GanttAXVD.ocx

set BaseDir="C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2"
set BinDir="%BaseDir%\Bin\Debug"
set Version=92.1.021
call "%VS110COMNTOOLS%vsvars32.bat"

msbuild "Factura Electronica VK.sln" /t:Clean,Build  /p:Configuration="Debug SAP910 x64" /p:PlatformTarget=x64
set BUILD_STATUS=%ERRORLEVEL%
if %BUILD_STATUS%==0 GOTO Reactor
pause
EXIT

:Reactor
"C:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.exe" -project "C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\bin\Debug\Factura Electronica.nrproj" -targetfile "C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\bin\Debug\Factura Electronica.exe"

REM "C:\Program Files\Eziriz\.NET Reactor\dotNET_Reactor.exe" -project %BinDir%\CMMSOne.nrproj -targetfile %BinDir%\CMMSOne.exe REM COMENTARIO

set REACTOR_STATUS=%ERRORLEVEL%
if %REACTOR_STATUS%==0 GOTO INNO
pause
EXIT

:INNO
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Factura Electronica x64.iss"
set INNO_STATUS=%ERRORLEVEL%
if %INNO_STATUS%==0 GOTO ARD
pause
EXIT

:ARD 
"C:\Program Files (x86)\SAP\SAP Business One SDK\Tools\AddOnRegDataGen\AddOnRegDataGen.exe" "C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Output\Factura Electronica SAP910x64.xml" %Version% "C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Output\setup.exe" "C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\Output\setup.exe" "C:\Users\ajose\Documents\VisualK\Proyectos\Factura Electronica VK 2\bin\Debug\Factura Electronica.exe"
ECHO %ERRORLEVEL%
pause