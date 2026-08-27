%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe %~dp0HangfireService.exe
Net Start HangfireForTaskJob
sc config HangfireForTaskJob start= auto
pause