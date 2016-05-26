@echo off
cls

if "%1" == "quickrun" (
  <%= packagesPath %>\FAKE\tools\FAKE.exe run --fsiargs -d:NO_FSI_ADDPRINTER build.fsx
) else (
  <%= paketPath %>\paket.bootstrapper.exe
  if errorlevel 1 (
    exit /b %errorlevel%
  )
  if not exist paket.lock (
    <%= paketPath %>\paket.exe install
  ) else (
    <%= paketPath %>\paket.exe restore
  )
  if errorlevel 1 (
    exit /b %errorlevel%
  )
  <%= packagesPath %>\FAKE\tools\FAKE.exe %* --fsiargs -d:NO_FSI_ADDPRINTER build.fsx
)
