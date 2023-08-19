using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Jace.DemoApp
{
    public partial class InputDialog : Window
    {
        private readonly TaskCompletionSource<double?> taskCompletionSource;
        
        public InputDialog()
        {
            InitializeComponent();
            taskCompletionSource = new TaskCompletionSource<double?>();
        }


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            questionLabel = this.FindControl<TextBlock>("questionLabel");
            valueTextBox = this.FindControl<TextBox>("valueTextBox");
            
            if (questionLabel == null || valueTextBox == null)
            {
                throw new InvalidOperationException("Could not find required controls.");
            }
            
            valueTextBox.AttachedToVisualTree += (_, _) => valueTextBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(valueTextBox.Text, out double value))
            {
                taskCompletionSource.SetResult(value); // Set the result of the dialog
            }
            else
            {
                taskCompletionSource.SetResult(null); // Indicate that the input was invalid
                Console.WriteLine($"Invalid input: {valueTextBox.Text}. Will use 0.0 instead.");
            }

            Close();
        }
        
        public async Task<double?> ShowDialogAsync(string variableName, Window parent)
        {
            questionLabel.Text = $"Please provide a value for variable \"{variableName}\":";
            await ShowDialog(parent);
            return await taskCompletionSource.Task;
        }
    }
}
