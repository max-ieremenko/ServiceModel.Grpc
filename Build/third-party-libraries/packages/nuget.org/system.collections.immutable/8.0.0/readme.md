System.Collections.Immutable [8.0.0](https://www.nuget.org/packages/System.Collections.Immutable/8.0.0)
--------------------

Used by: DesignTime internal

Target frameworks: net6.0

License: [MIT](../../../../licenses/mit) 

- package license: [MIT](https://licenses.nuget.org/MIT) 
- repository license: [MIT](https://github.com/dotnet/runtime) 
- project license: [Unknown](https://dot.net/) 

Description
-----------
This package provides collections that are thread safe and guaranteed to never change their contents, also known as immutable collections. Like strings, any methods that perform modifications will not change the existing instance but instead return a new instance. For efficiency reasons, the implementation uses a sharing mechanism to ensure that newly created instances share as much data as possible with the previous instance while ensuring that operations have a predictable time complexity.

The System.Collections.Immutable library is built-in as part of the shared framework in .NET Runtime. The package can be installed when you need to use it in other target frameworks.

Remarks
-----------
no remarks


Dependencies 1
-----------

|Name|Version|
|----------|:----|
|[System.Runtime.CompilerServices.Unsafe](../../../../packages/nuget.org/system.runtime.compilerservices.unsafe/6.0.0)|6.0.0|

*This page was generated by a tool.*