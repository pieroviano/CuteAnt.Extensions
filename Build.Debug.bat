del Artifacts\Debug\Net4x.*
for /d %%G in ("%userprofile%\.nuget\Packages\Net4x.*") do rd /s /q "%%~G"
nuget restore CuteAnt.Extensions.sln
MSBuild.exe CuteAnt.Extensions.sln /property:Configuration=Debug
git push
git add -A
git commit -a --allow-empty-message -m ''
git push
