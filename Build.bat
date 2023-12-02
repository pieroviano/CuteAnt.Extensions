IF NOT DEFINED Configuration SET Configuration=Release
if exist  Properties\Lock.txt del Properties\Lock.txt
if not exist "%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard1.1\" md "%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard1.1"
type NUL >%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard1.1\System.Buffers.dll
if not exist "%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard1.1\" md "%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard2.0"
type NUL >%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard2.0\System.Buffers.dll
MSBuild.exe Net4x.Extensions.sln -t:clean
MSBuild.exe Net4x.Extensions.sln -t:restore -p:RestorePackagesConfig=true /property:Configuration=%Configuration%
MSBuild.exe Net4x.Extensions.sln -m /property:Configuration=%Configuration%
git add -A
git commit -a --allow-empty-message -m ''
git push
