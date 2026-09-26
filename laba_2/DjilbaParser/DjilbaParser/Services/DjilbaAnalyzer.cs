using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DjilbaParser.Models;

namespace DjilbaParser.Services
{
    public sealed class DjilbaAnalyzer
    {
        public AnalysisResult Analyze(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Файл пуст или содержит только пробелы.");

            var tree = CSharpSyntaxTree.ParseText(
                source,
                new CSharpParseOptions(LanguageVersion.Latest));

            var diagnostics = tree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            if (diagnostics.Count > 0)
            {
                var message = new StringBuilder();
                message.AppendLine("Файл содержит синтаксические ошибки:");

                foreach (var diagnostic in diagnostics.Take(8))
                {
                    var line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
                    message.AppendLine($"Строка {line}: {diagnostic.GetMessage()}");
                }

                if (diagnostics.Count > 8)
                    message.AppendLine($"... и ещё {diagnostics.Count - 8} ошибок.");

                throw new InvalidOperationException(message.ToString());
            }

            var root = tree.GetCompilationUnitRoot();
            var allStatements = root.DescendantNodes()
                .OfType<StatementSyntax>()
                .Where(IsCountedOperator)
                .ToList();

            int totalOperators = allStatements.Count;
            int conditionalOperators = 0;
            int maxNesting = 0;

            foreach (var statement in allStatements)
            {
                int depth = GetStructuralDepth(statement);

                switch (statement)
                {
                    case IfStatementSyntax:
                        conditionalOperators++;
                        maxNesting = Math.Max(maxNesting, depth);
                        break;

                    case SwitchStatementSyntax switchStatement:
                        int caseCount = CountCaseLabels(switchStatement);

                        // Правило задания:
                        // n обычных case -> CL += n.
                        // default в CL не входит.
                        // Максимальный уровень, создаваемый switch, равен n - 1
                        // относительно уровня самого switch.
                        conditionalOperators += caseCount;

                        if (caseCount > 0)
                            maxNesting = Math.Max(maxNesting, depth + caseCount - 1);

                        break;
                }
            }

            double relativeComplexity = totalOperators == 0
                ? 0.0
                : (double)conditionalOperators / totalOperators;

            return new AnalysisResult(
                new DjilbaMetrics(
                    conditionalOperators,
                    totalOperators,
                    relativeComplexity,
                    maxNesting),
                diagnostics);
        }

        private static bool IsCountedOperator(StatementSyntax statement)
        {
            // { } — структурные блоки, а объявление локальной функции — декларация,
            // а не исполняемый оператор в используемом здесь классическом подсчёте.
            return statement is not BlockSyntax
                and not EmptyStatementSyntax
                and not LabeledStatementSyntax
                and not LocalFunctionStatementSyntax;
        }

        private static int CountCaseLabels(SwitchStatementSyntax switchStatement)
        {
            return switchStatement.Sections
                .SelectMany(section => section.Labels)
                .Count(IsCaseLabel);
        }

        private static bool IsCaseLabel(SwitchLabelSyntax label)
        {
            return label is CaseSwitchLabelSyntax or CasePatternSwitchLabelSyntax;
        }

        private static int GetStructuralDepth(StatementSyntax statement)
        {
            int depth = 0;

            foreach (var ancestor in statement.Ancestors())
            {
                switch (ancestor)
                {
                    case IfStatementSyntax:
                    case ForStatementSyntax:
                    case ForEachStatementSyntax:
                    case ForEachVariableStatementSyntax:
                    case WhileStatementSyntax:
                    case DoStatementSyntax:
                        depth++;
                        break;

                    case SwitchStatementSyntax switchStatement:
                        depth += GetSwitchBranchDepth(switchStatement, statement);
                        break;
                }
            }

            return depth;
        }

        private static int GetSwitchBranchDepth(
            SwitchStatementSyntax switchStatement,
            StatementSyntax nestedStatement)
        {
            var section = nestedStatement.Ancestors()
                .OfType<SwitchSectionSyntax>()
                .FirstOrDefault(s => s.Parent == switchStatement);

            if (section is null)
                return 0;

            int caseCount = CountCaseLabels(switchStatement);

            int branchIndex;
            int ordinaryCasesBeforeSection = switchStatement.Sections
                .TakeWhile(s => s != section)
                .SelectMany(s => s.Labels)
                .Count(IsCaseLabel);

            bool containsDefault = section.Labels.Any(l => l is DefaultSwitchLabelSyntax);

            if (containsDefault)
            {
                // default — последняя ветка цепочки, после всех case.
                branchIndex = Math.Max(0, caseCount - 1);
            }
            else
            {
                branchIndex = ordinaryCasesBeforeSection;
            }

            // Первая ветка эквивалентна if на уровне switch;
            // последующие ветки становятся вложенными else-if.
            return branchIndex + 1;
        }
    }

    public sealed record AnalysisResult(DjilbaMetrics Metrics, IReadOnlyList<Diagnostic> Diagnostics) { }
}
