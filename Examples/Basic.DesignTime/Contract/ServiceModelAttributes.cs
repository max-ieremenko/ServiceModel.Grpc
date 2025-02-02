/*
   Option 1: reference System.ServiceModel.Primitives NuGet package in the .csproj
   <ItemGroup>
     <PackageReference Include="System.ServiceModel.Primitives" />
   </ItemGroup>

   Option 2: mimic required attributes in your code
 */
namespace System.ServiceModel;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
internal sealed class ServiceContractAttribute : Attribute
{
    public string? Name { get; set; }

    public string? Namespace { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class OperationContractAttribute : Attribute
{
    public string? Name { get; set; }
}