IF NOT DEFINED Configuration SET Configuration=Release
if exist  Properties\Lock.txt del Properties\Lock.txt
if not exist "%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard1.1\" md "%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard1.1"
type NUL >%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard1.1\System.Buffers.dll
if not exist "%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard1.1\" md "%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard2.0"
type NUL >%USERPROFILE%\.nuget\packages\system.buffers\4.4.0\lib\netstandard2.0\System.Buffers.dll
del Properties/Lock.txt
MSBuild.exe Net4x.Extensions.sln -t:clean
MSBuild.exe Net4x.Extensions.sln -t:restore -p:RestorePackagesConfig=true /property:Configuration=%Configuration%
MSBuild.exe Net4x.Extensions.sln -m /property:Configuration=%Configuration%
del Properties/Lock.txt
IF DEFINED Package (
	cd Packages
	FOR %%i IN (Net4x.AspNetCore.JsonPatch.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Buffers.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Collections.Immutable.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Caching.Abstractions.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Caching.Memory.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.Abstractions.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.Binder.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.CommandLine.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.EnvironmentVariables.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.FileExtensions.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.Ini.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.Json.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Configuration.Xml.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.DependencyInjection.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.DependencyInjection.Abstractions.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.FileProviders.Abstractions.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.FileProviders.Composite.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.FileProviders.Embedded.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.FileProviders.Physical.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.FileSystemGlobbing.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Logging.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Logging.Abstractions.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.ObjectPool.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Options.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Options.ConfigurationExtensions.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Options.DataAnnotations.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Extensions.Primitives.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Reflection.Metadata.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Security.Claims.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Text.Encodings.Web.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	FOR %%i IN (Net4x.Text.RegularExpressions.*.nupkg) DO dotnet nuget push %%i --source NuGet --api-key %ApiKey% -t 1000
	cd ..
)
git add -A
git commit -a --allow-empty-message -m ''
git push
