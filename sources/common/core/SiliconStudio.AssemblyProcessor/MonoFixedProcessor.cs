// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Linq;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Temporary workaround for https://github.com/xamarin/xamarin-android/issues/1177.
    /// </summary>
    internal class MonoFixedProcessor : IAssemblyDefinitionProcessor
    {
        public bool Process(AssemblyProcessorContext context)
        {
            if (context.Platform != Core.PlatformType.Android && context.Platform != Core.PlatformType.iOS)
                return false;

            bool changed = false;
            foreach (var type in context.Assembly.MainModule.GetAllTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (method.Body == null)
                        continue;

                    for (var index = 0; index < method.Body.Instructions.Count; index++)
                    {
                        var instruction = method.Body.Instructions[index];
                        if (instruction.OpCode == OpCodes.Conv_U)
                        {
                            VariableDefinition variable = null;

                            // Check if next store is in a pointer variable
                            switch (instruction.Next.OpCode.Code)
                            {
                                case Code.Stloc_0:
                                    variable = method.Body.Variables[0];
                                    break;
                                case Code.Stloc_1:
                                    variable = method.Body.Variables[1];
                                    break;
                                case Code.Stloc_2:
                                    variable = method.Body.Variables[2];
                                    break;
                                case Code.Stloc_3:
                                    variable = method.Body.Variables[3];
                                    break;
                                case Code.Stloc:
                                    variable = (VariableDefinition)instruction.Operand;
                                    break;
                            }

                            if (variable != null && variable.VariableType.IsPointer)
                            {
                                // We are in a fixed instruction, let's fix it
                                instruction.OpCode = OpCodes.Conv_I;
                                changed = true;
                            }
                        }
                    }
                }
            }

            return changed;
        }
    }
}
