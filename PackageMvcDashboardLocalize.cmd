@ECHO OFF

:: SEE ALSO: https://docs.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-template-package
:: SEE ALSO: https://github.com/sayedihashimi/template-sample#how-to-ship-a-template-inside-of-a-visual-studio-extension-vsix
:: SEE ALSO: https://www.c-sharpcorner.com/article/how-to-create-a-vsix-extension-for-a-custom-template-project/

XCOPY /S /Y "%~dp0MyMvcApp\Areas\MvcDashboardLocalize" "%~dp0packaging\MvcDashboardLocalize\templates\MvcDashboardLocalize\Areas\MvcDashboardLocalize\"
XCOPY /S /Y "%~dp0MyMvcApp\Data\Localize" "%~dp0packaging\MvcDashboardLocalize\templates\MvcDashboardLocalize\Data\Localize\"
XCOPY /S /Y "%~dp0MyMvcApp\Localize" "%~dp0packaging\MvcDashboardLocalize\templates\MvcDashboardLocalize\Localize\"
COPY /Y "%~dp0MyMvcApp\ModelStateLocalization.json" "%~dp0packaging\MvcDashboardLocalize\templates\MvcDashboardLocalize\ModelStateLocalization.json"
ECHO.
ECHO Edit the template file then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardLocalize\templates\MvcDashboardLocalize\.template.config\template.json"
ECHO Edit the project file (version nr etc) then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardLocalize\templatepack.csproj"
PUSHD "%~dp0packaging\MvcDashboardLocalize"
DOTNET pack
ECHO.
ECHO Package(s) created:
DIR bin\Debug\*.nupkg /b /s
ECHO.
POPD
PAUSE