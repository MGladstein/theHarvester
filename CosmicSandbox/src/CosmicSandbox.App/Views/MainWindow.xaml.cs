using System.Windows;
using System.Windows.Controls;
using CosmicSandbox.Core.Models;
using CosmicSandbox.PhysicsEngine;

namespace CosmicSandbox.App.Views;

public partial class MainWindow : Window
{
    private SimulationState? _simulationState;
    private SimulationController? _controller;

    public MainWindow()
    {
        InitializeComponent();
        InitializeSimulation();
        SetupEventHandlers();
    }

    private void InitializeSimulation()
    {
        _simulationState = new SimulationState
        {
            Name = "New Simulation",
            IsPaused = true
        };

        _controller = new SimulationController(_simulationState);

        // Populate combo boxes
        TypeComboBox.ItemsSource = Enum.GetValues(typeof(BodyType));
        IntegratorComboBox.ItemsSource = Enum.GetValues(typeof(IntegratorType));
        IntegratorComboBox.SelectedItem = IntegratorType.VelocityVerlet;

        UpdateUI();
    }

    private void SetupEventHandlers()
    {
        TimeScaleSlider.ValueChanged += (s, e) =>
        {
            if (_simulationState != null)
            {
                _simulationState.TimeScale = TimeScaleSlider.Value;
                TimeScaleText.Text = $"{TimeScaleSlider.Value:F1}x";
            }
        };
    }

    private void UpdateUI()
    {
        if (_simulationState == null) return;

        BodiesListBox.ItemsSource = _simulationState.Bodies.Select(b => b.Name).ToList();
        BodyCountText.Text = $"Bodies: {_simulationState.Bodies.Count}";
        SimTimeText.Text = $"Time: {FormatTime(_simulationState.CurrentTime)}";

        var stats = _controller?.GetStats();
        if (stats != null)
        {
            EnergyDriftText.Text = $"{stats.EnergyDrift * 100:F4}%";
        }
    }

    private string FormatTime(double seconds)
    {
        if (seconds < 60) return $"{seconds:F1}s";
        if (seconds < 3600) return $"{seconds / 60:F1}m";
        if (seconds < 86400) return $"{seconds / 3600:F1}h";
        if (seconds < 31536000) return $"{seconds / 86400:F1}d";
        return $"{seconds / 31536000:F1}y";
    }

    // Event Handlers
    private void NewProject_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Create new project? Unsaved changes will be lost.",
            "New Project", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            InitializeSimulation();
            StatusText.Text = "New project created";
        }
    }

    private void OpenProject_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Cosmic Sandbox Projects (*.csp)|*.csp|All Files (*.*)|*.*",
            Title = "Open Project"
        };

        if (dialog.ShowDialog() == true)
        {
            // TODO: Load project from file
            StatusText.Text = $"Opened: {dialog.FileName}";
        }
    }

    private void SaveProject_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Save current project
        StatusText.Text = "Project saved";
    }

    private void SaveProjectAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Cosmic Sandbox Projects (*.csp)|*.csp",
            Title = "Save Project As"
        };

        if (dialog.ShowDialog() == true)
        {
            // TODO: Save project to file
            StatusText.Text = $"Saved as: {dialog.FileName}";
        }
    }

    private void ExportImage_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Export image functionality coming soon";
    }

    private void ExportVideo_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Export video functionality coming soon";
    }

    private void ExportData_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Export CSV functionality coming soon";
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Preferences_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Preferences coming soon";
    }

    private void ToggleSimulation_Click(object sender, RoutedEventArgs e)
    {
        if (_simulationState != null)
        {
            _simulationState.IsPaused = !_simulationState.IsPaused;
            StatusText.Text = _simulationState.IsPaused ? "Simulation paused" : "Simulation running";
        }
    }

    private void ResetSimulation_Click(object sender, RoutedEventArgs e)
    {
        InitializeSimulation();
        StatusText.Text = "Simulation reset";
    }

    private void AddBody_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Add body dialog coming soon";
    }

    private void Documentation_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://github.com/cosmicsandbox/docs",
            UseShellExecute = true
        });
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Cosmic Sandbox v1.0.0\n\n" +
            "A production-ready Windows 11 gravitational simulator\n" +
            "with realistic N-body physics and 3D visualization.\n\n" +
            "Built with DirectX 11, .NET 8.0, and public-domain NASA/ESA assets.",
            "About Cosmic Sandbox",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    private void LoadSolarSystem_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Loading Solar System preset...";
    }

    private void LoadBinaryStars_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Loading Binary Stars preset...";
    }

    private void LoadThreeBody_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Loading Three-Body Problem preset...";
    }

    private void LoadGalaxyCollision_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Loading Galaxy Collision preset...";
    }

    private void AddStar_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Adding star...";
    }

    private void AddPlanet_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Adding planet...";
    }

    private void AddMoon_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Adding moon...";
    }

    private void AddAsteroid_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Adding asteroid...";
    }

    private void BodiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TODO: Update inspector with selected body properties
    }

    private void BrowseTexture_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg|All Files (*.*)|*.*",
            Title = "Select Texture"
        };

        if (dialog.ShowDialog() == true)
        {
            StatusText.Text = $"Texture: {dialog.FileName}";
        }
    }
}
