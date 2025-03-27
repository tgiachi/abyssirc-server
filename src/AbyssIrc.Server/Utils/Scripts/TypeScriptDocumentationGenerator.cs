using System.Reflection;
using System.Text;
using AbyssIrc.Core.Attributes.Scripts;
using AbyssIrc.Core.Data.Internal.Scripts;
using AbyssIrc.Core.Extensions;
using HamLink.Core.Attributes.Scripts;

namespace AbyssIrc.Server.Utils.Scripts;

public static class TypeScriptDocumentationGenerator
{
    private static readonly HashSet<Type> _processedTypes = new();
    private static readonly StringBuilder _interfacesBuilder = new();
    private static readonly StringBuilder _constantsBuilder = new();


    public static string GenerateDocumentation(List<ScriptModuleData> scriptModules, Dictionary<string, object> constants)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/**");
        sb.AppendLine(" * AbyssIRC Server JavaScript API TypeScript Definitions");
        sb.AppendLine(" * Auto-generated documentation");
        sb.AppendLine(" **/");
        sb.AppendLine();

        // Reset processed types and interfaces builder for this generation run
        _processedTypes.Clear();
        _interfacesBuilder.Clear();


        ProcessConstants(constants);

        sb.Append(_constantsBuilder.ToString());

        foreach (var module in scriptModules)
        {
            var scriptModuleAttribute = module.ModuleType.GetCustomAttribute<ScriptModuleAttribute>();

            if (scriptModuleAttribute == null)
            {
                continue;
            }

            var moduleName = scriptModuleAttribute.Name;

            sb.AppendLine($"/**");
            sb.AppendLine($" * {module.ModuleType.Name} module");
            sb.AppendLine($" */");
            sb.AppendLine($"declare const {moduleName}: {{");


            // Get all methods with ScriptFunction attribute
            var methods = module.ModuleType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<ScriptFunctionAttribute>() != null)
                .ToList();

            foreach (var method in methods)
            {
                var scriptFunctionAttr = method.GetCustomAttribute<ScriptFunctionAttribute>();

                if (scriptFunctionAttr == null)
                {
                    continue;
                }

                var functionName = method.Name;
                var description = scriptFunctionAttr.HelpText;

                // Generate function documentation
                sb.AppendLine($"    /**");
                sb.AppendLine($"     * {description}");

                // Add parameter documentation
                var parameters = method.GetParameters();
                foreach (var param in parameters)
                {
                    var paramType = ConvertToTypeScriptType(param.ParameterType);
                    sb.AppendLine($"     * @param {param.Name} {paramType}");
                }

                // Add return type documentation if not void
                if (method.ReturnType != typeof(void))
                {
                    var returnType = ConvertToTypeScriptType(method.ReturnType);
                    sb.AppendLine($"     * @returns {returnType}");
                }

                sb.AppendLine($"     */");

                // Generate function signature
                sb.Append($"    {functionName}(");

                // Generate parameters
                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var paramType = ConvertToTypeScriptType(param.ParameterType);
                    var isOptional = param.IsOptional || param.ParameterType.IsByRef ||
                                     param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() ==
                                     typeof(Nullable<>) ||
                                     paramType.EndsWith("[]?");

                    sb.Append($"{param.Name}{(isOptional ? "?" : "")}: {paramType}");

                    if (i < parameters.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }

                // Add return type
                var methodReturnType = ConvertToTypeScriptType(method.ReturnType);
                sb.AppendLine($"): {methodReturnType};");
            }

            sb.AppendLine("};");
            sb.AppendLine();
        }

        // Add all generated interfaces
        var interfacesText = _interfacesBuilder.ToString();

        // Adjust indentation for interfaces (remove 4 spaces from each line)
        var lines = interfacesText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        for (var i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                if (lines[i].StartsWith("    "))
                {
                    lines[i] = lines[i].Substring(4);
                }
            }
        }

        sb.Append(string.Join(Environment.NewLine, lines));

        return sb.ToString();
    }


    private static string ConvertToTypeScriptType(Type type)
    {
        if (type == typeof(void))
        {
            return "void";
        }

        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(float) ||
            type == typeof(double) || type == typeof(decimal))
        {
            return "number";
        }

        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (type == typeof(object))
        {
            return "any";
        }

        if (type == typeof(object[]))
        {
            return "any[]";
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return $"{ConvertToTypeScriptType(elementType!)}[]";
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return $"{ConvertToTypeScriptType(underlyingType!)} | null";
        }

        // Handle params object[]? case
        if (type.IsArray && type.GetElementType() == typeof(object) && type.Name.EndsWith("[]"))
        {
            return "any[]?";
        }

        // Handle Dictionary<TKey, TValue>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var genericArgs = type.GetGenericArguments();
            var keyType = ConvertToTypeScriptType(genericArgs[0]);
            var valueType = ConvertToTypeScriptType(genericArgs[1]);

            // For string keys, use standard record type
            if (genericArgs[0] == typeof(string))
            {
                return $"{{ [key: string]: {valueType} }}";
            }

            // For other keys, use Map
            return $"Map<{keyType}, {valueType}>";
        }

        // Handle Action delegates
        if (type == typeof(Action))
        {
            return "() => void";
        }

        // Handle generic Actions with up to 8 type parameters
        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();

            // Action<T1>
            if (genericTypeDefinition == typeof(Action<>))
            {
                var typeArg = type.GetGenericArguments()[0];
                return $"(arg: {ConvertToTypeScriptType(typeArg)}) => void";
            }

            // Action<T1, T2>
            if (genericTypeDefinition == typeof(Action<,>))
            {
                var typeArgs = type.GetGenericArguments();
                return
                    $"(arg1: {ConvertToTypeScriptType(typeArgs[0])}, arg2: {ConvertToTypeScriptType(typeArgs[1])}) => void";
            }

            // Action<T1, T2, T3>
            if (genericTypeDefinition == typeof(Action<,,>))
            {
                var typeArgs = type.GetGenericArguments();
                return
                    $"(arg1: {ConvertToTypeScriptType(typeArgs[0])}, arg2: {ConvertToTypeScriptType(typeArgs[1])}, arg3: {ConvertToTypeScriptType(typeArgs[2])}) => void";
            }

            // Action<T1, T2, T3, T4>
            if (genericTypeDefinition == typeof(Action<,,,>))
            {
                var typeArgs = type.GetGenericArguments();
                return
                    $"(arg1: {ConvertToTypeScriptType(typeArgs[0])}, arg2: {ConvertToTypeScriptType(typeArgs[1])}, arg3: {ConvertToTypeScriptType(typeArgs[2])}, arg4: {ConvertToTypeScriptType(typeArgs[3])}) => void";
            }

            // Handle Func delegates
            if (genericTypeDefinition == typeof(Func<>))
            {
                var returnType = type.GetGenericArguments()[0];
                return $"() => {ConvertToTypeScriptType(returnType)}";
            }

            if (genericTypeDefinition == typeof(Func<,>))
            {
                var typeArgs = type.GetGenericArguments();
                return $"(arg: {ConvertToTypeScriptType(typeArgs[0])}) => {ConvertToTypeScriptType(typeArgs[1])}";
            }

            if (genericTypeDefinition == typeof(Func<,,>))
            {
                var typeArgs = type.GetGenericArguments();
                return
                    $"(arg1: {ConvertToTypeScriptType(typeArgs[0])}, arg2: {ConvertToTypeScriptType(typeArgs[1])}) => {ConvertToTypeScriptType(typeArgs[2])}";
            }

            // Continue with existing generic type handling
            if (genericTypeDefinition == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                return $"{ConvertToTypeScriptType(underlyingType!)} | null";
            }

            if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                var genericArgs = type.GetGenericArguments();
                var keyType = ConvertToTypeScriptType(genericArgs[0]);
                var valueType = ConvertToTypeScriptType(genericArgs[1]);

                // For string keys, use standard record type
                if (genericArgs[0] == typeof(string))
                {
                    return $"{{ [key: string]: {valueType} }}";
                }

                // For other keys, use Map
                return $"Map<{keyType}, {valueType}>";
            }

            if (genericTypeDefinition == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                return $"{ConvertToTypeScriptType(elementType)}[]";
            }
        }

        // Handle List<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = type.GetGenericArguments()[0];
            return $"{ConvertToTypeScriptType(elementType)}[]";
        }

        // For complex types (classes and structs), generate interfaces
        if ((type.IsClass || type.IsValueType) && !type.IsPrimitive && !type.IsEnum && type.Namespace != null &&
            !type.Namespace.StartsWith("System"))
        {
            // Generate interface name
            var interfaceName = $"I{type.Name}";

            // If we've already processed this type, just return the interface name
            if (_processedTypes.Contains(type))
            {
                return interfaceName;
            }

            // Mark type as processed to prevent infinite recursion
            _processedTypes.Add(type);

            // Start building the interface
            _interfacesBuilder.AppendLine();
            _interfacesBuilder.AppendLine($"/**");
            _interfacesBuilder.AppendLine($" * Generated interface for {type.FullName}");
            _interfacesBuilder.AppendLine($" */");
            _interfacesBuilder.AppendLine($"interface {interfaceName} {{");

            // Get properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();

            foreach (var property in properties)
            {
                var propertyType = ConvertToTypeScriptType(property.PropertyType);

                // Add property documentation
                _interfacesBuilder.AppendLine($"    /**");
                _interfacesBuilder.AppendLine($"     * {property.Name}");
                _interfacesBuilder.AppendLine($"     */");

                // Add property
                _interfacesBuilder.AppendLine($"    {property.Name.ToCamelCase()}: {propertyType};");
            }

            // End interface
            _interfacesBuilder.AppendLine("}");

            return interfaceName;
        }

        // Handle enums
        if (type.IsEnum)
        {
            GenerateEnumInterface(type);
            return type.Name;
        }

        if (typeof(Delegate).IsAssignableFrom(type))
        {
            var method = type.GetMethod("Invoke");
            if (method != null)
            {
                var parameters = method.GetParameters();
                var paramStrings = parameters.Select((p, i) => $"arg{i}: {ConvertToTypeScriptType(p.ParameterType)}");
                var returnType = ConvertToTypeScriptType(method.ReturnType);
                return $"({string.Join(", ", paramStrings)}) => {returnType}";
            }

            return "(...args: any[]) => any";
        }

        // For other complex types, return any
        return "any";
    }

    private static string FormatConstantValue(object value, Type type)
    {
        if (value == null)
        {
            return "null";
        }

        if (type == typeof(string))
        {
            return $"\"{value}\"";
        }

        if (type == typeof(bool))
        {
            return value.ToString().ToLower();
        }

        if (type.IsEnum)
        {
            return $"{type.Name}.{value}";
        }

        // For numerical values and other types
        return value.ToString();
    }


    private static void ProcessConstants(Dictionary<string, object> constants)
    {
        if (constants.Count == 0)
        {
            return;
        }

        _constantsBuilder.AppendLine("// Constants");
        _constantsBuilder.AppendLine();

        foreach (var constant in constants)
        {
            var constantName = constant.Key;
            var constantValue = constant.Value;
            var constantType = constantValue?.GetType() ?? typeof(object);

            var typeScriptType = ConvertToTypeScriptType(constantType);
            var formattedValue = FormatConstantValue(constantValue, constantType);

            // Generate constant documentation
            _constantsBuilder.AppendLine($"/**");
            _constantsBuilder.AppendLine($" * {constantName} constant ");
            _constantsBuilder.AppendLine($" * \"{formattedValue}\"");
            _constantsBuilder.AppendLine($" */");
            _constantsBuilder.AppendLine($"declare const {constantName}: {typeScriptType};");
            _constantsBuilder.AppendLine();
        }

        _constantsBuilder.AppendLine();
    }

    private static void GenerateEnumInterface(Type enumType)
    {
        if (!_processedTypes.Add(enumType))
        {
            return;
        }

        _interfacesBuilder.AppendLine();
        _interfacesBuilder.AppendLine($"/**");
        _interfacesBuilder.AppendLine($" * Generated enum for {enumType.FullName}");
        _interfacesBuilder.AppendLine($" */");
        _interfacesBuilder.AppendLine($"enum {enumType.Name} {{");

        var enumValues = Enum.GetNames(enumType);

        foreach (var value in enumValues)
        {
            var numericValue = (int)Enum.Parse(enumType, value);
            _interfacesBuilder.AppendLine($"    {value} = {numericValue},");
        }

        _interfacesBuilder.AppendLine("}");
    }
}
