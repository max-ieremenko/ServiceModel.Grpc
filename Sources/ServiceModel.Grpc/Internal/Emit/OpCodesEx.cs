using System;
using System.Reflection.Emit;

namespace ServiceModel.Grpc.Internal.Emit
{
    internal static class OpCodesEx
    {
        public static void EmitLdcI4(this ILGenerator body, int value)
        {
            switch (value)
            {
                case 0:
                    body.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    body.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    body.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    body.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    body.Emit(OpCodes.Ldc_I4_4);
                    return;
            }

            throw new NotImplementedException();
        }

        public static void EmitLdarg(this ILGenerator body, int argumentIndex)
        {
            switch (argumentIndex)
            {
                case 0:
                    body.Emit(OpCodes.Ldarg_0);
                    return;

                case 1:
                    body.Emit(OpCodes.Ldarg_1);
                    return;

                case 2:
                    body.Emit(OpCodes.Ldarg_2);
                    return;
                
                case 3:
                    body.Emit(OpCodes.Ldarg_3);
                    return;
            }

            if (argumentIndex <= 255)
            {
                body.Emit(OpCodes.Ldarg_S, argumentIndex);
            }
            else
            {
                body.Emit(OpCodes.Ldarg, argumentIndex);
            }
        }
    }
}
