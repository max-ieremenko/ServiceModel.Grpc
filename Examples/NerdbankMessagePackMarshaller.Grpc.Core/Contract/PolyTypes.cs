using PolyType;
using PolyType.SourceGenerator;

namespace Contract;

public static class PolyTypes
{
    // share access to the generated TypeShapeProvider of this assembly
    public static ITypeShapeProvider TypeShapeProvider => ShapeProvider_Contract.Default;
}