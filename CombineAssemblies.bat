@echo on
rem set targetDir=C:\WorkRoot\AbotV1.1\Abot\bin\Debug\
set targetDir=%1
set tempDir=%targetDir%IlMergeTemp\

rd /Q /S %tempDir%

mkdir %tempDir%
move /Y %targetDir%Abot.dll %tempDir%
move /Y %targetDir%AutoMapper.dll %tempDir%
move /Y %targetDir%HtmlAgilityPack.dll %tempDir%

"C:\Program Files (x86)\Microsoft\ILMerge\ilmerge.exe" /targetplatform:v4 /wildcards /internalize /target:library /out:%targetDir%Abot.dll %tempDir%Abot.dll %tempDir%AutoMapper.dll %tempDir%HtmlAgilityPack.dll

rd /Q /S %tempDir%
del /Q /F %targetDir%AutoMapper.*
del /Q /F %targetDir%HtmlAgilityPack.*