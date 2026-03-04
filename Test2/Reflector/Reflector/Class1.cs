namespace Reflector;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

public class Reflector
{
    public static void PrintStructure(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (!type.IsClass)
        {
            return;
        }

        string code = GenerateTypeCode(type, isTopLevel: true);
        File.WriteAllText($"{type.Name}.cs", code);
    }
    
    private static string GenerateTypeCode(Type type, bool isTopLevel, string indent = "")
    {
        var sb = new StringBuilder();

        string visibility = GetVisibility(type);
        sb.AppendLine($"{indent}{visibility}class {type.Name}");
        sb.AppendLine($"{indent}{{");

        string innerIndent = indent + "    ";
        
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | 
                                            BindingFlags.Instance | BindingFlags.Static | 
                                            BindingFlags.DeclaredOnly))
        {
            string fieldVisibility = field.IsPublic ? "public " : "private ";
            string isStatic = field.IsStatic ? "static " : "";
            sb.AppendLine($"{innerIndent}{fieldVisibility}{isStatic}{field.FieldType.Name} {field.Name};");
        }
        
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
                                              BindingFlags.Instance | BindingFlags.Static | 
                                              BindingFlags.DeclaredOnly)
                                  .Where(m => !m.IsSpecialName))
        {
            string methodVisibility = method.IsPublic ? "public " : "private ";
            string isStatic = method.IsStatic ? "static " : "";
            string parameters = string.Join(", ", method.GetParameters()
                .Select(p => $"{p.ParameterType.Name} {p.Name}"));
            
            sb.AppendLine($"{innerIndent}{methodVisibility}{isStatic}{method.ReturnType.Name} {method.Name}({parameters})");
            sb.AppendLine($"{innerIndent}{{");
            
            if (method.ReturnType != typeof(void))
            {
                if (method.ReturnType.IsValueType)
                    sb.AppendLine($"{innerIndent}    return default;");
                else
                    sb.AppendLine($"{innerIndent}    return null;");
            }
            
            sb.AppendLine($"{innerIndent}}}");
            sb.AppendLine();
        }
        
        
        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | 
                                                      BindingFlags.DeclaredOnly)
                                      .Where(t => t.IsClass))
        {
            sb.Append(GenerateTypeCode(nestedType, isTopLevel: false, indent: innerIndent));
            sb.AppendLine();
        }
        
        sb.AppendLine($"{indent}}}");
        return sb.ToString();
    }

    private static string GetVisibility(Type type)
    {
        if (type.IsNested)
        {
            if (type.IsNestedPublic) return "public ";
            if (type.IsNestedPrivate) return "private ";
            return "private ";
        }
        return "public ";
    }

    /// <summary>
    /// Сравнивает два типа и выводит различия в полях и методах.
    /// Вывод направляется в указанный TextWriter (по умолчанию — Console.Out).
    /// </summary>
    public static void DiffClasses(Type a, Type b, TextWriter output = null)
    {
        output ??= Console.Out;

        if (a == null || b == null)
        {
            throw new ArgumentNullException();
        }

        var fieldsA = a.GetFields(BindingFlags.Public | BindingFlags.NonPublic | 
                                 BindingFlags.Instance | BindingFlags.Static | 
                                 BindingFlags.DeclaredOnly);
        var fieldsB = b.GetFields(BindingFlags.Public | BindingFlags.NonPublic | 
                                 BindingFlags.Instance | BindingFlags.Static | 
                                 BindingFlags.DeclaredOnly);
        
        var methodsA = a.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
                                   BindingFlags.Instance | BindingFlags.Static | 
                                   BindingFlags.DeclaredOnly)
                       .Where(m => !m.IsSpecialName).ToArray();
        var methodsB = b.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | 
                                   BindingFlags.Instance | BindingFlags.Static | 
                                   BindingFlags.DeclaredOnly)
                       .Where(m => !m.IsSpecialName).ToArray();

        
        var nestedA = a.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | 
                                      BindingFlags.DeclaredOnly)
                      .Where(t => t.IsClass).ToArray();
        var nestedB = b.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | 
                                      BindingFlags.DeclaredOnly)
                      .Where(t => t.IsClass).ToArray();

        var onlyInA_Fields = fieldsA.Where(fA => !fieldsB.Any(fB => 
            fB.Name == fA.Name && 
            fB.FieldType == fA.FieldType && 
            fB.IsStatic == fA.IsStatic && 
            fB.IsPublic == fA.IsPublic));
        
        var onlyInB_Fields = fieldsB.Where(fB => !fieldsA.Any(fA => 
            fA.Name == fB.Name && 
            fA.FieldType == fB.FieldType && 
            fA.IsStatic == fB.IsStatic && 
            fA.IsPublic == fB.IsPublic));
        
        var onlyInA_Methods = methodsA.Where(mA => !methodsB.Any(mB => 
            mB.Name == mA.Name && 
            mB.ReturnType == mA.ReturnType &&
            string.Join(",", mB.GetParameters().Select(p => p.ParameterType.Name)) == 
            string.Join(",", mA.GetParameters().Select(p => p.ParameterType.Name))));
        
        var onlyInB_Methods = methodsB.Where(mB => !methodsA.Any(mA => 
            mA.Name == mB.Name && 
            mA.ReturnType == mB.ReturnType &&
            string.Join(",", mA.GetParameters().Select(p => p.ParameterType.Name)) == 
            string.Join(",", mB.GetParameters().Select(p => p.ParameterType.Name))));
        
        var onlyInA_Nested = nestedA.Where(nA => !nestedB.Any(nB => nB.Name == nA.Name));
        var onlyInB_Nested = nestedB.Where(nB => !nestedA.Any(nA => nA.Name == nB.Name));

        output.WriteLine($"Сравнение {a.Name} и {b.Name}");
        output.WriteLine();

        if (onlyInA_Fields.Any())
        {
            output.WriteLine($"Поля только в {a.Name}:");
            foreach (var field in onlyInA_Fields)
            {
                output.WriteLine($"  {field.FieldType.Name} {field.Name}");
            }
        }

        if (onlyInB_Fields.Any())
        {
            output.WriteLine($"\nПоля только в {b.Name}:");
            foreach (var field in onlyInB_Fields)
            {
                output.WriteLine($"  {field.FieldType.Name} {field.Name}");
            }
        }

        if (onlyInA_Methods.Any())
        {
            output.WriteLine($"\nМетоды только в {a.Name}:");
            foreach (var method in onlyInA_Methods)
            {
                string parameters = string.Join(", ", method.GetParameters()
                    .Select(p => $"{p.ParameterType.Name} {p.Name}"));
                output.WriteLine($"  {method.ReturnType.Name} {method.Name}({parameters})");
            }
        }

        if (onlyInB_Methods.Any())
        {
            output.WriteLine($"\nМетоды только в {b.Name}:");
            foreach (var method in onlyInB_Methods)
            {
                string parameters = string.Join(", ", method.GetParameters()
                    .Select(p => $"{p.ParameterType.Name} {p.Name}"));
                output.WriteLine($"  {method.ReturnType.Name} {method.Name}({parameters})");
            }
        }

        if (onlyInA_Nested.Any())
        {
            output.WriteLine($"\nВложенные классы только в {a.Name}:");
            foreach (var nested in onlyInA_Nested)
                output.WriteLine($"  {nested.Name}");
        }

        if (onlyInB_Nested.Any())
        {
            output.WriteLine($"\nВложенные классы только в {b.Name}:");
            foreach (var nested in onlyInB_Nested)
                output.WriteLine($"  {nested.Name}");
        }
    }
}