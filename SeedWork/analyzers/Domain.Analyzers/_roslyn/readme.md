These files have been copied from the [otnet/runtime](https://github.com/dotnet/runtime) repository, in an attempt to make the analyzers pack as expected.
They have been taken over 1:1 except for the //<auto-generated /> and #nullable enable headers, to ensure they are not modified by any code cleanup.
All files have been copied from either \src\libraries\Common\src\Roslyn or \src\libraries\System.Private.CoreLib\src\System\Collections\Generic

These files are necessary to make certain features that have been implemented using Roslyn API v4.4 compatible with the now used version of Roslyn 4.0, which is the base
version of VS 2022. 

It could be possible remove these files again after a refactoring of the existing code, but for now this has to do
