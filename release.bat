@ECHO OFF
SET scriptDir=%~dp0
SET psScript=Deploy.ps1
SET pwshScriptPath=%scriptDir%%psScript%

pwsh -NoProfile -ExecutionPolicy Bypass -Command "& '%pwshScriptPath%'";
