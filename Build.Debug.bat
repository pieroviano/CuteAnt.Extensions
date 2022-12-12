del Artifacts\Debug\Net4x.*
rmdir /s /q %userprofile%\.nuget\Packages\Net4x.*
nuget restore CuteAnt.Extensions.sln
MSBuild.exe CuteAnt.Extensions.sln /property:Configuration=Debug
git push
git add -A
git commit -a --allow-empty-message -m ''
git push
