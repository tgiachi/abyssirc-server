using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Server.Core.Attributes.Scripts;
using AbyssIrc.Server.Core.Data.Internal.ScriptEngine;

namespace AbyssIrc.Server.Utils;

public static class TypeScriptDocumentationGenerator
{
    private static readonly HashSet<Type> _processedTypes = [];
    private static readonly StringBuilder _interfacesBuilder = new();
    private static readonly StringBuilder _constantsBuilder = new();
    private static readonly StringBuilder _enumsBuilder = new();
    private static readonly List<Type> _interfaceTypesToGenerate = [];

    public static List<Type> FoundEnums { get; } = [];

    public static void AddInterfaceToGenerate(Type type)
    {
        _interfaceTypesToGenerate.Add(type);
    }

    private static Func<string, string> _nameResolver = name => name.ToSnakeCase();

    public static string GenerateDocumentation(
        string appName, string appVersion, List<ScriptModuleData> scriptModules, Dictionary<string, object> constants,
        Func<string, string> nameResolver = null
    )
    {
        if (nameResolver != null)
        {
            _nameResolver = nameResolver;
        }

        var sb = new StringBuilder();
        sb.AppendLine("/**");
        sb.AppendLine($" * {appName} v{appVersion} JavaScript API TypeScript Definitions");
        sb.AppendLine(" * Auto-generated documentation on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine(" **/");
        sb.AppendLine();

        /// Reset processed types and builders for this generation run
        _processedTypes.Clear();
        _interfacesBuilder.Clear();
        _constantsBuilder.Clear();
        _enumsBuilder.Clear();
        //_interfaceTypesToGenerate.Clear();

        var distinctConstants = constants
            .GroupBy(kvp => kvp.Key)
            .ToDictionary(g => g.Key, g => g.First().Value);

        ProcessConstants(distinctConstants);

        sb.Append(_constantsBuilder);

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

            /// Get all methods with ScriptFunction attribute
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

                var functionName = _nameResolver(method.Name);
                var description = scriptFunctionAttr.HelpText;

                /// Generate function documentation
                sb.AppendLine($"    /**");
                sb.AppendLine($"     * {description}");

                /// Add parameter documentation
                var parameters = method.GetParameters();
                foreach (var param in parameters)
                {
                    var paramType = ConvertToTypeScriptType(param.ParameterType);
                    sb.AppendLine($"     * @param {_nameResolver(param.Name)} {paramType}");
                }

                /// Add return type documentation if not void
                if (method.ReturnType != typeof(void))
                {
                    var returnType = ConvertToTypeScriptType(method.ReturnType);
                    sb.AppendLine($"     * @returns {returnType}");
                }

                sb.AppendLine($"     */");

                /// Generate function signature
                sb.Append($"    {functionName}(");

                /// Generate parameters
                for (var i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var paramType = ConvertToTypeScriptType(param.ParameterType);
                    var isOptional = param.IsOptional || param.ParameterType.IsByRef ||
                                     param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() ==
                                     typeof(Nullable<>) ||
                                     paramType.EndsWith("[]?");

                    sb.Append($"{_nameResolver(param.Name)}{(isOptional ? "?" : "")}: {paramType}");

                    if (i < parameters.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }

                /// Add return type
                var methodReturnType = ConvertToTypeScriptType(method.ReturnType);
                sb.AppendLine($"): {methodReturnType};");
            }

            sb.AppendLine("};");
            sb.AppendLine();
        }

        /// Now generate all the interfaces that were collected during type conversion
        GenerateAllInterfaces();

        /// First append all enums, then append all interfaces
        sb.Append(string.Join(Environment.NewLine, _enumsBuilder));
        sb.AppendLine();
        sb.Append(string.Join(Environment.NewLine, _interfacesBuilder));

        return sb.ToString();
    }

    /// <summary>
    /// Method to generate all interfaces after collecting them
    /// </summary>
    private static void GenerateAllInterfaces()
    {
        /// Use a more thorough approach to handle dependencies between types
        bool processedSomething;

        do
        {
            /// Create a copy of the list to avoid "Collection was modified" exception
            var typesToGenerate = new List<Type>(_interfaceTypesToGenerate);

            /// Keep track of whether we processed any types in this iteration
            processedSomething = false;

            /// Process types not yet processed
            foreach (var type in typesToGenerate)
            {
                /// Skip if already processed
                if (!_processedTypes.Contains(type))
                {
                    GenerateInterface(type);
                    processedSomething = true;
                }
            }

            /// Continue until no new types are processed
        } while (processedSomething);
    }

    /// <summary>
    /// Check if a type is a C# record type
    /// </summary>
    private static bool IsRecordType(Type type)
    {
        /// C# records have specific characteristics:
        /// 1. They are classes
        /// 2. They have a compiler-generated EqualityContract property
        /// 3. They have specific compiler-generated methods

        if (!type.IsClass)
            return false;

        /// Check for the EqualityContract property which is generated for all record types
        var equalityContract = type.GetProperty(
            "EqualityContract",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (equalityContract != null && equalityContract.PropertyType == typeof(Type))
        {
            return true;
        }

        /// Alternative check: look for compiler-generated attributes or methods
        /// Records have compiler-generated ToString, GetHashCode, Equals methods
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var hasCompilerGeneratedToString = methods.Any(m =>
            m.Name == "ToString" &&
            m.GetParameters().Length == 0 &&
            m.GetCustomAttributes().Any(attr => attr.GetType().Name.Contains("CompilerGenerated"))
        );

        return hasCompilerGeneratedToString;
    }

    /// <summary>
    /// Method to generate a single interface
    /// </summary>
    private static void GenerateInterface(Type type)
    {
        if (!_processedTypes.Add(type))
        {
            return; /// Already processed
        }

        var interfaceName = $"I{type.Name}";

        /// Start building the interface
        _interfacesBuilder.AppendLine();
        _interfacesBuilder.AppendLine($"/**");

        if (IsRecordType(type))
        {
            _interfacesBuilder.AppendLine($" * Generated interface for record type {type.FullName}");
        }
        else
        {
            _interfacesBuilder.AppendLine($" * Generated interface for {type.FullName}");
        }

        _interfacesBuilder.AppendLine($" */");
        _interfacesBuilder.AppendLine($"interface {interfaceName} {{");

        /// Get properties - for records, we want to include both public properties and constructor parameters
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        /// For record types, also check constructor parameters to ensure we get all record properties
        if (IsRecordType(type))
        {
            /// Get the primary constructor (the one with the most parameters, typically the record constructor)
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var primaryConstructor = constructors.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

            if (primaryConstructor != null)
            {
                var constructorParams = primaryConstructor.GetParameters();
                foreach (var param in constructorParams)
                {
                    /// Check if we already have a property with this name
                    var existingProperty = properties.FirstOrDefault(p =>
                        string.Equals(p.Name, param.Name, StringComparison.OrdinalIgnoreCase)
                    );

                    if (existingProperty == null)
                    {
                        /// Add this parameter as a property in our interface
                        var paramType = ConvertToTypeScriptType(param.ParameterType);

                        _interfacesBuilder.AppendLine($"    /**");
                        _interfacesBuilder.AppendLine($"     * {_nameResolver(param.Name)} (from record constructor)");
                        _interfacesBuilder.AppendLine($"     */");
                        _interfacesBuilder.AppendLine($"    {_nameResolver(param.Name)}: {paramType};");
                    }
                }
            }
        }

        foreach (var property in properties)
        {
            var propertyType = ConvertToTypeScriptType(property.PropertyType);

            /// Add property documentation
            _interfacesBuilder.AppendLine($"    /**");
            _interfacesBuilder.AppendLine($"     * {_nameResolver(property.Name)}");
            _interfacesBuilder.AppendLine($"     */");

            /// Add property
            _interfacesBuilder.AppendLine($"    {_nameResolver(property.Name)}: {propertyType};");
        }

        /// End interface - make sure it's properly closed
        _interfacesBuilder.AppendLine("}");
    }

    private static string ConvertToTypeScriptType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
        Type type
    )
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

        /// Handle params object[]? case
        if (type.IsArray && type.GetElementType() == typeof(object) && type.Name.EndsWith("[]"))
        {
            return "any[]?";
        }

        /// Handle generic types
        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericArgs = type.GetGenericArguments();

            /// Handle Dictionary<TKey, TValue>
            if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                var keyType = ConvertToTypeScriptType(genericArgs[0]);
                var valueType = ConvertToTypeScriptType(genericArgs[1]);

                /// For string keys, use standard record type
                if (genericArgs[0] == typeof(string))
                {
                    return $"{{ [key: string]: {valueType} }}";
                }

                /// For other keys, use Map
                return $"Map<{keyType}, {valueType}>";
            }

            /// Handle Record<TKey, TValue> - questo è il fix principale
            if (type.Name.StartsWith("Record`") && genericArgs.Length == 2)
            {
                var keyType = ConvertToTypeScriptType(genericArgs[0]);
                var valueType = ConvertToTypeScriptType(genericArgs[1]);

                /// For string keys, use TypeScript Record type
                if (genericArgs[0] == typeof(string))
                {
                    return $"Record<string, {valueType}>";
                }

                /// For other key types, use TypeScript Record type
                return $"Record<{keyType}, {valueType}>";
            }

            /// Handle Action delegates
            if (genericTypeDefinition == typeof(Action<>))
            {
                var typeArg = genericArgs[0];
                return $"(arg: {ConvertToTypeScriptType(typeArg)}) => void";
            }

            if (genericTypeDefinition == typeof(Action<,>))
            {
                return
                    $"(arg1: {ConvertToTypeScriptType(genericArgs[0])}, arg2: {ConvertToTypeScriptType(genericArgs[1])}) => void";
            }

            if (genericTypeDefinition == typeof(Action<,,>))
            {
                return
                    $"(arg1: {ConvertToTypeScriptType(genericArgs[0])}, arg2: {ConvertToTypeScriptType(genericArgs[1])}, arg3: {ConvertToTypeScriptType(genericArgs[2])}) => void";
            }

            if (genericTypeDefinition == typeof(Action<,,,>))
            {
                return
                    $"(arg1: {ConvertToTypeScriptType(genericArgs[0])}, arg2: {ConvertToTypeScriptType(genericArgs[1])}, arg3: {ConvertToTypeScriptType(genericArgs[2])}, arg4: {ConvertToTypeScriptType(genericArgs[3])}) => void";
            }

            /// Handle Func delegates
            if (genericTypeDefinition == typeof(Func<>))
            {
                var returnType = genericArgs[0];
                return $"() => {ConvertToTypeScriptType(returnType)}";
            }

            if (genericTypeDefinition == typeof(Func<,>))
            {
                return $"(arg: {ConvertToTypeScriptType(genericArgs[0])}) => {ConvertToTypeScriptType(genericArgs[1])}";
            }

            if (genericTypeDefinition == typeof(Func<,,>))
            {
                return
                    $"(arg1: {ConvertToTypeScriptType(genericArgs[0])}, arg2: {ConvertToTypeScriptType(genericArgs[1])}) => {ConvertToTypeScriptType(genericArgs[2])}";
            }

            /// Handle List<T>
            if (genericTypeDefinition == typeof(List<>))
            {
                var elementType = genericArgs[0];
                return $"{ConvertToTypeScriptType(elementType)}[]";
            }
        }

        /// Handle Action without parameters
        if (type == typeof(Action))
        {
            return "() => void";
        }

        /// Handle C# record types explicitly
        if (IsRecordType(type))
        {
            /// Generate interface name for record
            var interfaceName = $"I{type.Name}";

            /// If we've already processed this type, just return the interface name
            if (_processedTypes.Contains(type))
            {
                return interfaceName;
            }

            /// Add this type to our list of types that need interfaces generated
            if (!_interfaceTypesToGenerate.Contains(type))
            {
                _interfaceTypesToGenerate.Add(type);
            }

            return interfaceName;
        }

        /// For complex types (classes and structs), generate interfaces
        if ((type.IsClass || type.IsValueType) && !type.IsPrimitive && !type.IsEnum && type.Namespace != null &&
            !type.Namespace.StartsWith("System"))
        {
            /// Generate interface name
            var interfaceName = $"I{type.Name}";

            /// If we've already processed this type, just return the interface name
            if (_processedTypes.Contains(type))
            {
                return interfaceName;
            }

            /// Add this type to our list of types that need interfaces generated
            if (!_interfaceTypesToGenerate.Contains(type))
            {
                _interfaceTypesToGenerate.Add(type);
            }

            return interfaceName;
        }

        /// Handle enums
        if (type.IsEnum)
        {
            GenerateEnumInterface(type);
            return _nameResolver(type.Name);
        }

        /// Handle other delegate types
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

        /// For other complex types, return any
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
            return $"{_nameResolver(type.Name)}.{value}";
        }

        /// For numerical values and other types
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

            /// Generate constant documentation
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

        FoundEnums.Add(enumType);

        _enumsBuilder.AppendLine();
        _enumsBuilder.AppendLine("/**");
        _enumsBuilder.AppendLine($" * Generated enum for {enumType.FullName}");
        _enumsBuilder.AppendLine(" */");
        _enumsBuilder.AppendLine($"export enum {_nameResolver(enumType.Name)} {{");

        var enumValues = Enum.GetNames(enumType);

        foreach (var value in enumValues)
        {
            var numericValue = -1;
            try
            {
                numericValue = Convert.ToInt32(Enum.Parse(enumType, value));
            }
            catch (InvalidCastException)
            {
                /// Handle the case where the enum value is not an integer
                /// This can happen if the enum is defined with a different underlying type
                numericValue = (int)Enum.Parse(enumType, value);
            }

            _enumsBuilder.AppendLine($"    {value} = {numericValue},");
        }

        _enumsBuilder.AppendLine("}");
    }
}
