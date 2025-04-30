@ECHO OFF

:: SEE ALSO: https://docs.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-template-package
:: SEE ALSO: https://github.com/sayedihashimi/template-sample#how-to-ship-a-template-inside-of-a-visual-studio-extension-vsix
:: SEE ALSO: https://www.c-sharpcorner.com/article/how-to-create-a-vsix-extension-for-a-custom-template-project/

XCOPY /S /Y "%~dp0MyMvcApp\Areas\MvcDashboardContent" "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\Areas\MvcDashboardContent\"
MKDIR "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\Controllers"
COPY /Y "%~dp0MyMvcApp\Controllers\ContentController.cs" "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\Controllers\ContentController.cs"
XCOPY /S /Y "%~dp0MyMvcApp\Data\Content" "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\Data\Content\"
XCOPY /S /Y "%~dp0MyMvcApp\Models\Content" "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\Models\Content\"
XCOPY /S /Y "%~dp0MyMvcApp\Views\Content" "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\Views\Content\"
XCOPY /S /Y "%~dp0MyMvcApp\Views\Shared\DisplayTemplates\Content" "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\Views\Shared\DisplayTemplates\Content\"
XCOPY /S /Y "%~dp0MyMvcApp\Views\Shared\EditorTemplates\Content" "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\Views\Shared\EditorTemplates\Content\"
COPY /Y "%~dp0MyMvcApp\ContentHtmlExtensions.cs" "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\ContentHtmlExtensions.cs"
ECHO.
ECHO Edit the template file then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardContent\templates\MvcDashboardContent\.template.config\template.json"
ECHO Edit the project file (version nr etc) then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardContent\templatepack.csproj"
ECHO Edit the README file then save and close to proceed.
NOTEPAD "%~dp0packaging\MvcDashboardContent\docs\README.md"
PUSHD "%~dp0packaging\MvcDashboardContent"
DOTNET pack
ECHO.
ECHO Package(s) created:
DIR bin\Release\*.nupkg /b /s
ECHO.
POPD
PAUSE
EXPLORER /root,"%~dp0packaging\MvcDashboardContent\bin"