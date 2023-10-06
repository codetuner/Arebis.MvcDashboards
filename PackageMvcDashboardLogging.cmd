@ECHO OFF

:: SEE ALSO: https://docs.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-template-package
:: SEE ALSO: https://github.com/sayedihashimi/template-sample#how-to-ship-a-template-inside-of-a-visual-studio-extension-vsix
:: SEE ALSO: https://www.c-sharpcorner.com/article/how-to-create-a-vsix-extension-for-a-custom-template-project/

XCOPY /S /Y "%~dp0MyMvcApp\Areas\MvcDashboardLogging" "%~dp0packaging\MvcDashboardLogging\templates\MvcDashboardLogging\Areas\MvcDashboardLogging\"
XCOPY /S /Y "%~dp0MyMvcApp\Data\Logging" "%~dp0packaging\MvcDashboardLogging\templates\MvcDashboardLogging\Data\Logging\"
XCOPY /S /Y "%~dp0MyMvcApp\Logging" "%~dp0packaging\MvcDashboardLogging\templates\MvcDashboardLogging\Logging\"
ECHO.
ECHO Edit the template file then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardLogging\templates\MvcDashboardLogging\.template.config\template.json"
ECHO Edit the project file (version nr etc) then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardLogging\templatepack.csproj"
ECHO Edit the README file then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardLogging\docs\README.md"
PUSHD "%~dp0packaging\MvcDashboardLogging"
DOTNET pack
ECHO.
ECHO Package(s) created:
DIR bin\Debug\*.nupkg /b /s
ECHO.
POPD
PAUSE
EXPLORER /root,"%~dp0packaging\MvcDashboardLogging\bin"