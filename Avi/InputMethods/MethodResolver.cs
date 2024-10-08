﻿using Inputs.Misc;

using System.Reflection;

namespace Inputs.InputMethods;

internal class MethodResolver<InputType> where InputType : class {
    private static InputType FindInAssembly<T>(Assembly assembly) where T : class {
        if (assembly == null)
            return null;

        var inputMethods = assembly.GetTypes().Where(t => t.Equals(typeof(T))).ToList();
        var inputMethod = inputMethods.FirstOrDefault();

        if (inputMethod == null)
            return null;

        return Activator.CreateInstance(inputMethod) as InputType;
    }

    public static InputType GetMethodObjectFor<T>() where T : class {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().ToList()) {
            var result = FindInAssembly<T>(asm);

            if (result == null)
                continue;

            return result;
        }

        throw new InputMethodNotFoundException($"The input method of {typeof(T).FullName} does not exist.");
    }
}
