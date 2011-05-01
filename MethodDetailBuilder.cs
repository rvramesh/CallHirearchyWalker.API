/************************************************************************************************
 * DICLAIMER : The below code is hacked over a weekend. This is not of a production quality
 * and it is proved to be right and not wrong. Please use this code AS/IS and I am not 
 * responsible for damages caused.  Software distributed under the License is distributed 
 * on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied.
 * 
 * Written by Ramesh Vijayaraghavan, <contact@rvramesh.com>
 *
 *
 * You are free to use this in any way you want, in case you find this useful or working for you.
 * ***********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace CallHirearchyWalker.API
{
    static class MethodDetailBuilder
    {
        public static MethodDetail Create(MethodReference reference)
        {
            return Create(reference, null);
        }

        public static MethodDetail Create(MethodReference reference, List<MethodDetail> usedMethods)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(reference.DeclaringType.FullName);
            builder.Append("::");
            builder.Append(reference.Name);
            if (reference.HasGenericParameters)
            {
                GetGenericMethodInstanceFullName(reference.GenericParameters, builder);
            }
            GetParameters(reference, builder);


            string fullName = reference.FullName;
            return new MethodDetail(fullName, builder.ToString(), usedMethods,MethodDetailBuilder.Create(reference.DeclaringType));
        }

        private static DeclaringTypeDetail Create(TypeReference reference)
        {
            return new DeclaringTypeDetail(reference.FullName, reference.Name);
        }

        private static void GetParameters(MethodReference reference, StringBuilder builder)
        {
            builder.Append("(");
            if (reference.HasParameters)
            {
                Collection<ParameterDefinition> parameters = reference.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                {
                    ParameterDefinition definition = parameters[i];
                    
                    if (i > 0)
                    {
                        builder.Append(",");
                    }
                    if (definition.ParameterType.IsSentinel)
                    {
                        builder.Append("...,");
                    }

                    TypeReference parameterType = definition.ParameterType;
                    if (parameterType is ByReferenceType)
                    {
                        parameterType = (parameterType as ByReferenceType).ElementType;
                        if (definition.IsOut)
                        {
                            builder.Append("out ");
                        }
                        else
                        {
                            builder.Append("ref ");
                        }
                    }
                    

                    if ((parameterType.IsGenericInstance && (parameterType as GenericInstanceType).HasGenericArguments))
                    {
                        builder.Append(parameterType.Name.Substring(0, parameterType.Name.IndexOf("`")));
                        var paramType = parameterType as GenericInstanceType;
                        builder.Append("<");
                        for (int j = 0; j < paramType.GenericArguments.Count; j++)
                        {
                            if (j > 0)
                            {
                                builder.Append(",");
                            }
                            builder.Append(paramType.GenericArguments[j].Name);
                        }
                        builder.Append(">");
                    }
                    else
                    {
                        builder.Append(definition.ParameterType.Name.Replace("&",string.Empty));
                    }
                }
            }
            builder.Append(")");
        }

        private static void GetGenericMethodInstanceFullName(Collection<GenericParameter> genericParams, StringBuilder builder)
        {
            
                builder.Append("<");
                for (int i = 0; i < genericParams.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(",");
                    }
                    builder.Append(genericParams[i].FullName);
                }
                builder.Append(">");
            
        }
    }
}
