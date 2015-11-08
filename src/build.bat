@echo off
"packages\FAKE\tools\Fake.exe" build.fsx %*
exit /b %errorlevel%
