@ECHO OFF

:: SEE ALSO: https://docs.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-template-package
:: SEE ALSO: https://github.com/sayedihashimi/template-sample#how-to-ship-a-template-inside-of-a-visual-studio-extension-vsix
:: SEE ALSO: https://www.c-sharpcorner.com/article/how-to-create-a-vsix-extension-for-a-custom-template-project/

XCOPY /S /Y "%~dp0MyMvcApp\Areas\MvcDashboardIdentity" "%~dp0packaging\MvcDashboardIdentity\templates\MvcDashboardIdentity\Areas\MvcDashboardIdentity\"
ECHO.
ECHO Edit the template file then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardIdentity\templates\MvcDashboardIdentity\.template.config\template.json"
ECHO Edit the project file (version nr etc) then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardIdentity\templatepack.csproj"
PUSHD "%~dp0packaging\MvcDashboardIdentity"
DOTNET pack
ECHO.
ECHO Package(s) created:
DIR bin\Debug\*.nupkg /b /s
ECHO.
POPD
PAUSE