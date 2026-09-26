using System.IO;
using System.Windows;
using Microsoft.Win32;
using DjilbaParser.Services;

namespace DjilbaParser
{
    public partial class MainWindow : Window
    {
        private readonly DjilbaAnalyzer _analyzer = new();
        private string? _currentFilePath;

        public MainWindow()
        {
            InitializeComponent();
            LoadSample();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите исходный файл C#",
                Filter = "C# файлы (*.cs)|*.cs|Все файлы (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                CodeTextBox.Text = File.ReadAllText(dialog.FileName);
                _currentFilePath = dialog.FileName;
                FileNameText.Text = dialog.FileName;
                AnalyzeCurrentCode();
            }
            catch (Exception ex)
            {
                ShowError($"Не удалось открыть файл: {ex.Message}");
            }
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeCurrentCode();
        }

        private void SampleButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSample();
        }

        private void LoadSample()
        {
            var samplePath = Path.Combine(
                AppContext.BaseDirectory,
                "Sample",
                "AnalyzedProgram.cs");

            if (File.Exists(samplePath))
            {
                CodeTextBox.Text = File.ReadAllText(samplePath);
                _currentFilePath = samplePath;
                FileNameText.Text = "Встроенный пример: AnalyzedProgram.cs";
                AnalyzeCurrentCode();
                return;
            }

            ShowError("Встроенный пример не найден.");
        }

        private void AnalyzeCurrentCode()
        {
            try
            {
                var result = _analyzer.Analyze(CodeTextBox.Text);
                var metrics = result.Metrics;

                AbsoluteValueText.Text = metrics.ConditionalOperators.ToString();
                RelativeValueText.Text = metrics.RelativeComplexity.ToString("0.####");
                RelativeFormulaText.Text =
                    $"{metrics.ConditionalOperators} / {metrics.TotalOperators} = {metrics.RelativeComplexity:0.####}";
                NestingValueText.Text = metrics.MaxNestingLevel.ToString();
                OperatorsText.Text = $"Всего операторов N: {metrics.TotalOperators}";
                CasesText.Text = GetSwitchSummary(CodeTextBox.Text);

                StatusText.Text = _currentFilePath is null
                    ? "Анализ завершён."
                    : $"Анализ завершён: {Path.GetFileName(_currentFilePath)}";
            }
            catch (Exception ex)
            {
                ClearMetrics();
                ShowError(ex.Message);
            }
        }

        private string GetSwitchSummary(string source)
        {
            try
            {
                var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
                var switches = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax>()
                    .Select(s =>
                    {
                        int cases = s.Sections
                            .SelectMany(section => section.Labels)
                            .Count(label => label is Microsoft.CodeAnalysis.CSharp.Syntax.CaseSwitchLabelSyntax
                                or Microsoft.CodeAnalysis.CSharp.Syntax.CasePatternSwitchLabelSyntax);
                        bool hasDefault = s.Sections
                            .SelectMany(section => section.Labels)
                            .Any(label => label is Microsoft.CodeAnalysis.CSharp.Syntax.DefaultSwitchLabelSyntax);
                        return $"switch: {cases} case" + (hasDefault ? ", default есть" : ", default нет");
                    })
                    .ToList();

                return switches.Count == 0
                    ? "switch в файле не найден"
                    : "Switch: " + string.Join("; ", switches);
            }
            catch
            {
                return "Данные switch недоступны из-за ошибки разбора";
            }
        }

        private void ClearMetrics()
        {
            AbsoluteValueText.Text = "—";
            RelativeValueText.Text = "—";
            RelativeFormulaText.Text = "CL / число операторов";
            NestingValueText.Text = "—";
            OperatorsText.Text = "";
            CasesText.Text = "";
        }

        private void ShowError(string message)
        {
            StatusText.Text = message;
            MessageBox.Show(message, "Ошибка анализа", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}