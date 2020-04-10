using System.Reflection;
using System.Text;
using Mono.Reflection;
using Shouldly;

namespace ServiceModel.Grpc.TestApi
{
    public static class ReflectionTestExtensions
    {
        public static string Disassemble(this MethodBase method)
        {
            method.ShouldNotBeNull();

            var result = new StringBuilder();

            foreach (var instruction in method.GetInstructions())
            {
                result.AppendLine(instruction.ToString());
            }

            return result.ToString();
        }
    }
}
