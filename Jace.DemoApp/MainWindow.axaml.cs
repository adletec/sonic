using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Jace.Execution;
using Jace.Operations;
using Jace.Tokenizer;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Jace.DemoApp
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            FormulaTextBox = this.FindControl<TextBox>("FormulaTextBox");
            TokensTextBox = this.FindControl<TextBox>("TokensTextBox");
            AstTreeView = this.FindControl<TreeView>("AstTreeView");
            ResultTextBox = this.FindControl<TextBox>("ResultTextBox");

            if (FormulaTextBox == null || TokensTextBox == null || AstTreeView == null || ResultTextBox == null)
            {
                throw new InvalidOperationException("Could not find required controls.");
            }
            
            FormulaTextBox.AttachedToVisualTree += (_, _) => FormulaTextBox.Focus();
        }

        private async void calculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearScreen();

                var formula = FormulaTextBox.Text;

                var reader = new TokenReader();
                var tokens = reader.Read(formula);

                ShowTokens(tokens);

                IFunctionRegistry functionRegistry = new FunctionRegistry(false);
                
                var astBuilder = new AstBuilder(functionRegistry, false);
                var operation = astBuilder.Build(tokens);

                ShowAbstractSyntaxTree(operation);

                var variables = new Dictionary<string, double>();
                foreach (var variable in GetVariables(operation))
                {
                    var value = await AskValueOfVariable(variable);
                    variables.Add(variable.Name, value ?? 0.0);
                }

                IExecutor executor = new Interpreter();
                var result = executor.Execute(operation, null, null, variables);

                ResultTextBox.Text = result.ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard(
                    "Error", 
                    $"Error: {ex.Message}\n\nStacktrace:\n{ex.StackTrace}.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error);
                await messageBox.ShowAsync();
            }
        }

        private void ClearScreen()
        {
            TokensTextBox.Text = "";
            AstTreeView.Items.Clear();
            ResultTextBox.Text = "";
        }

        private void ShowTokens(IList<Token> tokens)
        { 
            var result = "[ ";

            for(var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i].Value;

                switch (token)
                {
                    case string:
                        result += "\"" + token + "\"";
                        break;
                    case char:
                        result += "'" + token + "'";
                        break;
                    case double:
                    case int:
                        result += token;
                        break;
                }

                if (i < (tokens.Count - 1))
                    result += ", ";
            }

            result += " ]";

            TokensTextBox.Text = result;
        }

        private void ShowAbstractSyntaxTree(Operation operation)
        {
            AstTreeView.Items.Clear();
            var item = CreateTreeViewItem(operation);
            AstTreeView.Items.Add(item);
        }

        private TreeViewItem CreateTreeViewItem(Operation operation)
        {
            var item = new TreeViewItem
            {
                Header = GetLabelText(operation)
            };

            switch (operation)
            {
                case Multiplication multiplication:
                    item.Items.Clear();
                    item.Items.Add(CreateTreeViewItem(multiplication.Argument1));
                    item.Items.Add(CreateTreeViewItem(multiplication.Argument2));
                    break;
                case Addition addition:
                    item.Items.Clear();
                    item.Items.Add(CreateTreeViewItem(addition.Argument1));
                    item.Items.Add(CreateTreeViewItem(addition.Argument2));
                    break;
                case Subtraction subtraction:
                    item.Items.Clear();
                    item.Items.Add(CreateTreeViewItem(subtraction.Argument1));
                    item.Items.Add(CreateTreeViewItem(subtraction.Argument2));
                    break;
                case Division division:
                    item.Items.Clear();
                    item.Items.Add(CreateTreeViewItem(division.Dividend));
                    item.Items.Add(CreateTreeViewItem(division.Divisor));
                    break;
                case Exponentiation exponentiation:
                    item.Items.Clear();
                    item.Items.Add(CreateTreeViewItem(exponentiation.Base));
                    item.Items.Add(CreateTreeViewItem(exponentiation.Exponent));
                    break;
                case Function function:
                {
                    item.Items.Clear();
                    foreach (var argument in function.Arguments)
                        item.Items.Add(CreateTreeViewItem(argument));
                    break;
                }
            }

            return item;
        }

        private string GetLabelText(Operation operation)
        {
            var name = operation.GetType().Name;
            var dataType = operation.DataType.ToString();
            var value = "";

            switch (operation)
            {
                case IntegerConstant integerConstant:
                    value = $"({integerConstant.Value})";
                    break;
                case FloatingPointConstant floatingPointConstant:
                    value = $"({floatingPointConstant.Value})";
                    break;
                case Variable variable:
                    value = $"({variable.Name})";
                    break;
                case Function function:
                    value = $"({function.FunctionName})";
                    break;
            }

            return $"{name}<{dataType}>{value}";
        }

        private IEnumerable<Variable> GetVariables(Operation operation)
        {
            var variables = new List<Variable>();
            GetVariables(operation, variables);
            return variables;
        }

        private void GetVariables(Operation operation, ICollection<Variable> variables)
        {
            if (!operation.DependsOnVariables) return;
            if (operation.GetType() == typeof(Variable))
            {
                variables.Add((Variable)operation);
            }
            else if (operation.GetType() == typeof(Addition))
            {
                var addition = (Addition)operation;
                GetVariables(addition.Argument1, variables);
                GetVariables(addition.Argument2, variables);
            }
            else if (operation.GetType() == typeof(Multiplication))
            {
                var multiplication = (Multiplication)operation;
                GetVariables(multiplication.Argument1, variables);
                GetVariables(multiplication.Argument2, variables);
            }
            else if (operation.GetType() == typeof(Subtraction))
            {
                var subtraction = (Subtraction)operation;
                GetVariables(subtraction.Argument1, variables);
                GetVariables(subtraction.Argument2, variables);
            }
            else if (operation.GetType() == typeof(Division))
            {
                var division = (Division)operation;
                GetVariables(division.Dividend, variables);
                GetVariables(division.Divisor, variables);
            }
            else if (operation.GetType() == typeof(Exponentiation))
            {
                var exponentiation = (Exponentiation)operation;
                GetVariables(exponentiation.Base, variables);
                GetVariables(exponentiation.Exponent, variables);
            }
            else if (operation.GetType() == typeof(Function))
            {
                var function = (Function)operation;
                foreach (var argument in function.Arguments)
                {
                    GetVariables(argument, variables);
                }
            }
        }

        private async Task<double?> AskValueOfVariable(Variable variable)
        {
            var dialog = new InputDialog();
            return await dialog.ShowDialogAsync(variable.Name, this);
        }
    }
}
