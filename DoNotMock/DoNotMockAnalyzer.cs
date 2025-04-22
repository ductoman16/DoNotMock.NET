using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DoNotMock;

/// <summary>
/// Analyzer that detects violations of the <see cref="DoNotMockAttribute"/> usage in test code.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotMockAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "DNMK001";
    private const string Title = "Type marked with [DoNotMock] should not be mocked";
    private const string MessageFormat = "Type '{0}' is marked with [DoNotMock] and should not be mocked";
    private const string Description = "Types marked with [DoNotMock] indicate that they should not be mocked in tests. Use real implementations or hand-written test doubles instead.";
    private const string Category = "Design";
    private const string HelpLinkUri = "https://github.com/ductoman16/DoNotMock.NET/blob/main/README.md";

    /// <summary>
    /// Version of the analyzer. Increment for each release.
    /// </summary>
    public const string Version = "1.0.0";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: HelpLinkUri);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for syntax node analysis
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
        var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
        
        if (typeInfo.Type is not INamedTypeSymbol createdType)
        {
            return;
        }

        // Check for Mock<T> pattern (Moq)
        if (IsMoqMock(createdType, out var mockedType) && mockedType is not null)
        {
            CheckTypeForDoNotMockAttribute(context, objectCreation, mockedType);
            return;
        }

        // Check for FakeItEasy pattern (A.Fake<T>)
        if (IsFakeItEasyFake(createdType, out mockedType) && mockedType is not null)
        {
            CheckTypeForDoNotMockAttribute(context, objectCreation, mockedType);
            return;
        }
    }

    private static bool IsMoqMock(INamedTypeSymbol type, out ITypeSymbol? mockedType)
    {
        mockedType = null;

        // Check if it's a Mock<T> type
        if (!type.Name.Equals("Mock", StringComparison.Ordinal) ||
            !type.ContainingNamespace.ToDisplayString().Equals("Moq", StringComparison.Ordinal) ||
            type.TypeArguments.Length != 1)
        {
            return false;
        }

        mockedType = type.TypeArguments[0];
        return true;
    }

    private static bool IsFakeItEasyFake(INamedTypeSymbol type, out ITypeSymbol? mockedType)
    {
        mockedType = null;

        // Check if it's a Fake<T> type from FakeItEasy
        if (!type.Name.Equals("Fake", StringComparison.Ordinal) ||
            !type.ContainingNamespace.ToDisplayString().Equals("FakeItEasy", StringComparison.Ordinal) ||
            type.TypeArguments.Length != 1)
        {
            return false;
        }

        mockedType = type.TypeArguments[0];
        return true;
    }

    private static void CheckTypeForDoNotMockAttribute(SyntaxNodeAnalysisContext context, SyntaxNode node, ITypeSymbol type)
    {
        // Check if the type or any of its base types/interfaces have the DoNotMock attribute
        var typesToCheck = new Stack<ITypeSymbol>();
        typesToCheck.Push(type);

        while (typesToCheck.Count > 0)
        {
            var currentType = typesToCheck.Pop();

            var attributes = currentType.GetAttributes();
            var doNotMockAttribute = attributes.FirstOrDefault(attr =>
                attr.AttributeClass?.Name == "DoNotMockAttribute" &&
                attr.AttributeClass.ContainingNamespace.ToDisplayString() == "DoNotMock");

            if (doNotMockAttribute != null)
            {
                // Get the message from the attribute if provided
                var message = doNotMockAttribute.ConstructorArguments.Length > 0
                    ? doNotMockAttribute.ConstructorArguments[0].Value?.ToString()
                    : null;

                var typeName = type.ToDisplayString();
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                
                if (!string.IsNullOrEmpty(message))
                {
                    properties.Add("Message", message);
                }

                var diagnostic = Diagnostic.Create(
                    Rule,
                    node.GetLocation(),
                    properties.ToImmutable(),
                    typeName);

                context.ReportDiagnostic(diagnostic);
                return;
            }

            // Add base type if it exists
            if (currentType.BaseType != null)
            {
                typesToCheck.Push(currentType.BaseType);
            }

            // Add interfaces
            foreach (var iface in currentType.Interfaces)
            {
                typesToCheck.Push(iface);
            }
        }
    }

    /// <summary>
    /// Analyzes invocation expressions to detect mock creation patterns.
    /// </summary>
    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

        if (methodSymbol == null)
        {
            return;
        }

        // Check for NSubstitute.For<T>() pattern
        if (IsNSubstituteFor(methodSymbol, out var mockedType) && mockedType is not null)
        {
            CheckTypeForDoNotMockAttribute(context, invocation, mockedType);
            return;
        }

        // Check for A.Fake<T>() pattern
        if (IsFakeItEasyAFake(methodSymbol, out mockedType) && mockedType is not null)
        {
            CheckTypeForDoNotMockAttribute(context, invocation, mockedType);
            return;
        }
    }

    private static bool IsNSubstituteFor(IMethodSymbol method, out ITypeSymbol? mockedType)
    {
        mockedType = null;

        // Check if it's Substitute.For<T>()
        if (!method.Name.Equals("For", StringComparison.Ordinal) ||
            !method.ContainingType.Name.Equals("Substitute", StringComparison.Ordinal) ||
            !method.ContainingType.ContainingNamespace.ToDisplayString().Equals("NSubstitute", StringComparison.Ordinal) ||
            method.TypeArguments.Length != 1)
        {
            return false;
        }

        mockedType = method.TypeArguments[0];
        return true;
    }

    private static bool IsFakeItEasyAFake(IMethodSymbol method, out ITypeSymbol? mockedType)
    {
        mockedType = null;

        // Check if it's A.Fake<T>()
        if (!method.Name.Equals("Fake", StringComparison.Ordinal) ||
            !method.ContainingType.Name.Equals("A", StringComparison.Ordinal) ||
            !method.ContainingType.ContainingNamespace.ToDisplayString().Equals("FakeItEasy", StringComparison.Ordinal) ||
            method.TypeArguments.Length != 1)
        {
            return false;
        }

        mockedType = method.TypeArguments[0];
        return true;
    }
} 