using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Jace.DemoApp;

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
        QuestionLabel = this.FindControl<TextBlock>("QuestionLabel");
        ValueTextBox = this.FindControl<TextBox>("ValueTextBox");
            
        if (QuestionLabel == null || ValueTextBox == null)
        {
            throw new InvalidOperationException("Could not find required controls.");
        }
            
        ValueTextBox.AttachedToVisualTree += (_, _) => ValueTextBox.Focus();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(ValueTextBox.Text, out double value))
        {
            taskCompletionSource.SetResult(value); // Set the result of the dialog
        }
        else
        {
            taskCompletionSource.SetResult(null); // Indicate that the input was invalid
            Console.WriteLine($"Invalid input: {ValueTextBox.Text}. Will use 0.0 instead.");
        }

        Close();
    }
        
    public async Task<double?> ShowDialogAsync(string variableName, Window parent)
    {
        QuestionLabel.Text = $"Please provide a value for variable \"{variableName}\":";
        await ShowDialog(parent);
        return await taskCompletionSource.Task;
    }
}