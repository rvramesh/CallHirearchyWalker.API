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
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CallHirearchyWalker.API
{
    public class CecilUtil
    {
        public static AssemblyDefinition GetAssembly(String assemblyToLoad)
        {
            try
            {
                return AssemblyDefinition.ReadAssembly(assemblyToLoad);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.getAssembly");
                return null;
            }
        }

        public static List<TypeDefinition> GetTypes(AssemblyDefinition assemblyDefinition)
        {
            try
            {
                var types = new List<TypeDefinition>();
                foreach (ModuleDefinition module in assemblyDefinition.Modules)
                    foreach (TypeDefinition type in GetTypes(module))
                        types.Add(type);
                return types;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.getTypes");
                return null;
            }
        }

        public static List<TypeDefinition> GetTypes(ModuleDefinition moduleDefinition)
        {
            try
            {
                var types = new List<TypeDefinition>();
                foreach (TypeDefinition typeDefinition in moduleDefinition.Types)
                    types.Add(typeDefinition);
                return types;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public static List<MethodDefinition> GetMethods(AssemblyDefinition assemblyToLoad)
        {
            try
            {
                var methods = new List<MethodDefinition>();
                foreach (TypeDefinition typeDefinition in GetTypes(assemblyToLoad))
                    foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                        methods.Add(methodDefinition);
                return methods;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.getMethods");
                return null;
            }
        }

        public static List<MethodDefinition> GetMethods(String assemblyToLoad)
        {
            try
            {
                return GetMethods(GetAssembly(assemblyToLoad));
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.getMethods");
                return null;
            }
        }

        public static List<MethodDetail> GetMethodsWithChildMethodsInsideAssembly(String assemblyToLoad, bool includeMethodDefinition, string filterValue)
        {
            try
            {
                Func<MethodReference, bool> condition = (item => item.FullName.Contains(filterValue ?? string.Empty));
                List<MethodDefinition> definitions = GetMethodsInsideAssembly(assemblyToLoad, includeMethodDefinition, condition);
                var methodsCalled = new List<MethodDetail>();
                foreach (var item in definitions)
                {
                    methodsCalled.Add(MethodDetailBuilder.Create(item, GetMethodsCalledInsideMethod(item, condition)));
                }
                return methodsCalled;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.getMethodsCalledInsideAssembly");
                return null;
            }
        }

        private static List<MethodDefinition> GetMethodsInsideAssembly(String assemblyToLoad, bool includeMethodDefinition, Func<MethodReference, bool> condition)
        {
            string directory = Path.GetDirectoryName(assemblyToLoad);
            BaseAssemblyResolver resolver = (GlobalAssemblyResolver.Instance as BaseAssemblyResolver);
            if (!resolver.GetSearchDirectories().Contains(directory))
            {
                resolver.AddSearchDirectory(directory);
            }

            var methods = new List<MethodDefinition>();
            foreach (MethodDefinition methodDefinition in GetMethods(assemblyToLoad))
            {
                if ((methodDefinition.HasBody || includeMethodDefinition) && (condition(methodDefinition)))
                {
                    //if (includeMethodsCalled)
                    //{
                    //    methodsCalled.Add(MethodDetailBuilder.Create(methodDefinition, GetMethodsCalledInsideMethod(methodDefinition, condition)));
                    //}
                    //else
                    //{
                    //    methodsCalled.Add(MethodDetailBuilder.Create(methodDefinition));
                    //}
                    methods.Add(methodDefinition);
                }
            }
            return methods;
        }

        public static List<MethodDetail> GetMethodsCalledInsideMethod(MethodDefinition methodDefinition, Func<MethodReference, bool> condition)
        {
            try
            {
                var methodsCalled = new List<MethodDetail>();
                if (methodDefinition.Body != null)
                {
                    SequencePoint currentSequencePoint = null;
                    foreach (Instruction instruction in methodDefinition.Body.Instructions)
                    {
                        currentSequencePoint = instruction.SequencePoint ?? currentSequencePoint;
                        if (instruction.Operand != null)
                        {
                            if (instruction.Operand is MethodReference)
                            {
                                MethodReference operand = (instruction.Operand as MethodReference);

                                try
                                {
                                    operand = operand.Resolve();
                                }
                                catch (AssemblyResolutionException)
                                {
                                }

                                if (operand == null)
                                    operand = instruction.Operand as MethodReference;

                                if (!operand.FullName.Equals(methodDefinition.FullName) && condition(operand))
                                {
                                    var itemToBeAdded = MethodDetailBuilder.Create(operand);
                                    if (!methodsCalled.Contains(itemToBeAdded))
                                    {
                                        methodsCalled.Add(itemToBeAdded);
                                    }
                                }
                            }

                        }
                    }
                }

                return methodsCalled;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.getMethodsCalledInsideMethod");
                return null;
            }
        }

        public static List<MethodDetail> GetMethodsInsideAssembly(String assemblyToLoad, bool includeMethodDefinition, string filterValue)
        {
            try
            {
                Func<MethodReference, bool> condition = (item => item.FullName.Contains(filterValue ?? string.Empty));
                List<MethodDefinition> definitions = GetMethodsInsideAssembly(assemblyToLoad, includeMethodDefinition, condition);
                var methodsCalled = new List<MethodDetail>();
                foreach (var item in definitions)
                {
                    methodsCalled.Add(MethodDetailBuilder.Create(item));
                }
                return methodsCalled;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.getMethodsCalledInsideAssembly");
                return null;
            }
        }

        public static List<MethodDetail> GetMethodsCallingOperationContracts(string assemblyToLoad)
        {
            try
            {
                List<MethodDefinition> definitions = GetMethodsInsideAssembly(assemblyToLoad, false, item => true);
                var methodsCalled = new List<MethodDetail>();
                foreach (var item in definitions)
                {
                    var operationContractsCalled = GetMethodsCalledInsideMethod(item, CecilUtil.IsOperationContract);
                    if (operationContractsCalled != null && operationContractsCalled.Count > 0)
                    {
                        methodsCalled.Add(MethodDetailBuilder.Create(item, operationContractsCalled));
                    }
                }
                return methodsCalled;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.GetMethodsCallingOperationContracts");
                return null;
            }
        }

        private static bool IsOperationContract(MethodReference reference)
        {
            if (reference.IsDefinition)
            {
                var typeReference = (reference as MethodDefinition).DeclaringType.BaseType;
                return typeReference != null && typeReference.Name.Equals("ClientBase`1") && !(reference as MethodDefinition).IsConstructor;
            }
            return false;
        }

        public static Dictionary<MethodDetail, bool> GetServiceLevelOperationContracts(string assemblyToLoad, string filter)
        {
            try
            {
                //List<MethodDefinition> definitions = GetMethodsInsideAssembly(assemblyToLoad, false, item => item.FullName.Contains(filter ?? string.Empty) && (item as MethodDefinition).DeclaringType.Interfaces.Any(mItem=>CecilUtil.HasServiceContractAttribute(mItem.Resolve())));
                List<MethodDefinition> definitions = GetMethodsInsideAssembly(assemblyToLoad, true, item => !(item as MethodDefinition).HasBody && item.FullName.Contains(filter ?? string.Empty) && CecilUtil.HasServiceContractAttribute((item as MethodDefinition).DeclaringType.Resolve()));
                Dictionary<MethodDetail, bool> result = new Dictionary<MethodDetail, bool>();

                foreach (var item in definitions)
                {
                    if(!item.IsConstructor)
                        result.Add(MethodDetailBuilder.Create(item), CecilUtil.IsServiceLevelOperationContract(item));    
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "in CecilUtils.GetMethodsCallingOperationContracts");
                return null;
            }
        }

        private static bool IsServiceLevelOperationContract(MethodReference mReference)
        {

            MethodDefinition reference = mReference as MethodDefinition;
            //explicit
            if (reference.HasOverrides && reference.Overrides.Count > 0)
            {
                foreach (var item in reference.Overrides)
                {
                    bool isOverridesOperationContract = item.Resolve().CustomAttributes.Any(attributeItem => attributeItem.AttributeType.Name.Equals("OperationContractAttribute"));
                    if (isOverridesOperationContract)
                        return true;
                }
            }


            //implicit
            string methodfullName = reference.FullName;
            string declaringType = reference.DeclaringType.FullName;

            foreach (var item in reference.DeclaringType.Interfaces)
            {
                TypeDefinition itemDefinition = item.Resolve();
                bool isInterfaceServiceContract = HasServiceContractAttribute(itemDefinition);
                if (isInterfaceServiceContract)
                {
                    bool hasMatchingOperationContract = itemDefinition.Methods.Any(methodDefinition => methodDefinition.FullName.Replace(methodDefinition.DeclaringType.FullName, declaringType).Equals(methodfullName));
                    if (hasMatchingOperationContract)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasServiceContractAttribute(TypeDefinition item)
        {
            bool isInterfaceServiceContract = item.CustomAttributes.Any(attributeItem => attributeItem.AttributeType.Name.Equals("ServiceContractAttribute"));
            return isInterfaceServiceContract;
        }
    }
}

