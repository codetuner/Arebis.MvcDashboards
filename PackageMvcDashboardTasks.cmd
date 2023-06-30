@ECHO OFF

:: SEE ALSO: https://docs.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-template-package
:: SEE ALSO: https://github.com/sayedihashimi/template-sample#how-to-ship-a-template-inside-of-a-visual-studio-extension-vsix
:: SEE ALSO: https://www.c-sharpcorner.com/article/how-to-create-a-vsix-extension-for-a-custom-template-project/

XCOPY /S /Y "%~dp0MyMvcApp\Areas\MvcDashboardTasks" "%~dp0packaging\MvcDashboardTasks\templates\MvcDashboardTasks\Areas\MvcDashboardTasks\"
XCOPY /S /Y "%~dp0MyMvcApp\Data\Tasks" "%~dp0packaging\MvcDashboardTasks\templates\MvcDashboardTasks\Data\Tasks\"
XCOPY /S /Y "%~dp0MyMvcApp\Tasks" "%~dp0packaging\MvcDashboardTasks\templates\MvcDashboardTasks\Tasks\"
ECHO.
ECHO Edit the template file then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardTasks\templates\MvcDashboardTasks\.template.config\template.json"
ECHO Edit the project file (version nr etc) then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardTasks\templatepack.csproj"
PUSHD "%~dp0packaging\MvcDashboardTasks"
DOTNET pack
ECHO.
ECHO Package(s) created:
DIR bin\Debug\*.nupkg /b /s
ECHO.
POPD
PAUSE