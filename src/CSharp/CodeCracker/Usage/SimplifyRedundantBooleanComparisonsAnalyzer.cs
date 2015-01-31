﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeCracker.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SimplifyRedundantBooleanComparisonsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CC0049";
        internal const string Title = "Simplify expression";
        internal const string MessageFormat = "You can remove this comparison.";
        internal const string Category = SupportedCategories.Usage;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            customTags: WellKnownDiagnosticTags.Unnecessary,
            helpLink: HelpLink.ForDiagnostic(DiagnosticId));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var comparison = (BinaryExpressionSyntax)context.Node;

            // Only handle the case where both operands of type bool; other cases involve
            // too much complexity to be able to deliver an accurate diagnostic confidently.
            var leftType = context.SemanticModel.GetTypeInfo(comparison.Left).Type;
            var rightType = context.SemanticModel.GetTypeInfo(comparison.Right).Type;
            if (!IsBoolean(leftType) || !IsBoolean(rightType))
                return;

            var leftConstant = context.SemanticModel.GetConstantValue(comparison.Left);
            var rightConstant = context.SemanticModel.GetConstantValue(comparison.Right);
            if (!leftConstant.HasValue && !rightConstant.HasValue)
                return;

            var diagnostic = Diagnostic.Create(Rule, comparison.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsBoolean(ITypeSymbol symbol)
        {
            return symbol != null && symbol.SpecialType == SpecialType.System_Boolean;
        }
    }
}