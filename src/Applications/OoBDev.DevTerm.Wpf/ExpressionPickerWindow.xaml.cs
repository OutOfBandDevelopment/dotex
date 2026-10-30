using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.DeviceManifests.Editing;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>
/// Hosts an <see cref="ExpressionPickerViewModel"/>: the expression text, a filterable list of the paths it may read
/// (double-click inserts at the caret), the language's functions, live diagnostics and the result against sample data.
/// </summary>
public partial class ExpressionPickerWindow : Window
{
    private readonly ExpressionPickerViewModel _viewModel;
    private bool _syncing;

    /// <summary>Creates the window over <paramref name="viewModel"/>.</summary>
    /// <param name="viewModel">The picker view model.</param>
    public ExpressionPickerWindow(ExpressionPickerViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _viewModel = viewModel;
        InitializeComponent();
        WpfTheme.Attach(this);

        foreach (var function in ExpressionPickerViewModel.ButtonFunctions)
        {
            var captured = function;
            var button = new Button { Content = function.Name, Margin = new Thickness(0, 0, 6, 0), Padding = new Thickness(10, 2, 10, 2), ToolTip = $"{function.Signature}: {function.Description}" };
            button.Click += (_, _) => Insert(captured);
            FunctionPanel.Children.Add(button);
        }

        var more = new ComboBox { MinWidth = 150, VerticalContentAlignment = VerticalAlignment.Center, ToolTip = "Text and list functions" };
        more.Items.Add("more functions...");
        foreach (var function in ExpressionPickerViewModel.MoreFunctions)
        {
            more.Items.Add(new ComboBoxItem { Content = function.Name, Tag = function, ToolTip = $"{function.Signature}: {function.Description}" });
        }

        more.SelectedIndex = 0;
        more.SelectionChanged += (_, _) =>
        {
            if (more.SelectedItem is ComboBoxItem { Tag: ExpressionFunction picked })
            {
                Insert(picked);
                more.SelectedIndex = 0;
            }
        };
        FunctionPanel.Children.Add(more);

        Title = _viewModel.Title;
        ExpressionLabel.Text = _viewModel.Prompt;
        FunctionPanel.Visibility = _viewModel.ShowsFunctions ? Visibility.Visible : Visibility.Collapsed;
        ResultText.Visibility = _viewModel.ShowsResult ? Visibility.Visible : Visibility.Collapsed;
        NextSampleButton.Visibility = _viewModel.ShowsResult ? Visibility.Visible : Visibility.Collapsed;
        PathList.ToolTip = _viewModel.ChooseHint;

        _viewModel.Changed += (_, _) => Refresh();
        Refresh();
        ExpressionBox.CaretIndex = _viewModel.CaretIndex;
        Loaded += (_, _) => ExpressionBox.Focus();
    }

    /// <summary>The expression chosen with OK, or null when cancelled.</summary>
    public string? Accepted { get; private set; }

    private void SyncCaret()
    {
        _viewModel.CaretIndex = ExpressionBox.CaretIndex;
        _viewModel.SelectionLength = ExpressionBox.SelectionLength;
    }

    private void Insert(ExpressionFunction function)
    {
        SyncCaret();
        _viewModel.InsertFunction(function);
        ExpressionBox.Focus();
    }

    private void Refresh()
    {
        _syncing = true;
        try
        {
            if (ExpressionBox.Text != _viewModel.Text)
            {
                ExpressionBox.Text = _viewModel.Text;
            }

            ExpressionBox.CaretIndex = _viewModel.CaretIndex;
            DiagnosticsText.Text = _viewModel.Diagnostics;
            if (_viewModel.IsEmpty || (_viewModel.IsValid && _viewModel.Warnings.Count == 0))
            {
                DiagnosticsText.ClearValue(TextBlock.ForegroundProperty);
            }
            else
            {
                DiagnosticsText.SetResourceReference(TextBlock.ForegroundProperty, WpfTheme.Key(ThemeRole.OutputError));
            }

            ResultText.Text = $"Sample result: {_viewModel.ResultText}";
            PathList.ItemsSource = _viewModel.Paths;
        }
        finally
        {
            _syncing = false;
        }
    }

    private void ExpressionBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_syncing)
        {
            _viewModel.Text = ExpressionBox.Text;
        }
    }

    private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_syncing)
        {
            _viewModel.Filter = FilterBox.Text;
        }
    }

    private void PathList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (PathList.SelectedItem is PickerPath path)
        {
            SyncCaret();
            _viewModel.InsertPath(path);
            ExpressionBox.Focus();
        }
    }

    private void NextSample_Click(object sender, RoutedEventArgs e) => _viewModel.NextSample();

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Accepted = _viewModel.Text;
        DialogResult = true;
    }
}
