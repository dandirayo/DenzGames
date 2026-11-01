using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DenzGames.Launcher.Models;
using DenzGames.Launcher.Services;

namespace DenzGames.Launcher;

public partial class MainWindow : Window
{
    private readonly ManifestService _manifestService = new();
    private readonly PrerequisiteService _prerequisiteService = new();
    private GameLaunchService? _gameLaunchService;
    private IReadOnlyDictionary<string, PrerequisiteDefinition> _prerequisites =
        new Dictionary<string, PrerequisiteDefinition>();
    private GameDefinition? _selectedGame;

    public ObservableCollection<GameDefinition> Games { get; } = [];
    public ObservableCollection<PrerequisiteStatus> PrerequisiteStatuses { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_OnLoaded;
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var catalog = await _manifestService.LoadAsync();
            _prerequisites = catalog.Prerequisites;
            _gameLaunchService = new GameLaunchService(catalog.RepositoryRoot);

            foreach (var game in catalog.Games)
            {
                Games.Add(game);
            }

            var groupedGames = CollectionViewSource.GetDefaultView(Games);
            groupedGames.GroupDescriptions.Add(new PropertyGroupDescription(nameof(GameDefinition.Category)));
            GameList.ItemsSource = groupedGames;
            PrerequisiteList.ItemsSource = PrerequisiteStatuses;
            LibraryCountText.Text = $"{Games.Count} game dalam library";
            FooterText.Text = $"DenzGames • {catalog.RepositoryRoot}";

            if (Games.Count > 0)
            {
                GameList.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Katalog gagal dimuat: {ex.Message}";
            MessageBox.Show(ex.Message, "DenzGames", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void GameList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedGame = GameList.SelectedItem as GameDefinition;
        if (_selectedGame is null)
        {
            return;
        }

        SelectedNameText.Text = _selectedGame.Name;
        SelectedMetaText.Text = $"{_selectedGame.Category}  •  {_selectedGame.Engine}  •  v{_selectedGame.Version}";
        SelectedUpdateText.Text = $"Terakhir diperbarui {_selectedGame.LastUpdatedDisplay}";
        SelectedDescriptionText.Text = _selectedGame.Description;
        await RefreshPrerequisitesAsync();
    }

    private async Task RefreshPrerequisitesAsync()
    {
        if (_selectedGame is null || _gameLaunchService is null)
        {
            return;
        }

        RefreshButton.IsEnabled = false;
        PlayButton.IsEnabled = false;
        StatusText.Text = "Memeriksa prasyarat dan build game...";
        PrerequisiteStatuses.Clear();

        foreach (var prerequisiteId in _selectedGame.Prerequisites)
        {
            if (!_prerequisites.TryGetValue(prerequisiteId, out var definition))
            {
                PrerequisiteStatuses.Add(PrerequisiteStatus.MissingDefinition(prerequisiteId));
                continue;
            }

            PrerequisiteStatuses.Add(await _prerequisiteService.CheckAsync(prerequisiteId, definition));
        }

        var requirementsReady = PrerequisiteStatuses.All(status => status.IsInstalled);
        var gameReady = _gameLaunchService.CanPrepare(_selectedGame, out var availabilityMessage);
        PlayButton.IsEnabled = requirementsReady && gameReady;
        StatusText.Text = requirementsReady
            ? availabilityMessage
            : "Ada prasyarat yang belum terdeteksi. Gunakan tombol Bantuan, lalu klik Periksa ulang.";
        RefreshButton.IsEnabled = true;
    }

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        await RefreshPrerequisitesAsync();
    }

    private async void PlayButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectedGame is null || _gameLaunchService is null)
        {
            return;
        }

        try
        {
            PlayButton.IsEnabled = false;
            StatusText.Text = _selectedGame.Archive is not null
                ? "Menyiapkan build game. Ekstraksi pertama mungkin memerlukan waktu..."
                : "Menjalankan game...";

            var executable = await _gameLaunchService.PrepareAndLaunchAsync(_selectedGame);
            StatusText.Text = $"{_selectedGame.Name} dijalankan dari {executable}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Game gagal dijalankan: {ex.Message}";
            MessageBox.Show(ex.Message, "DenzGames", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            PlayButton.IsEnabled = PrerequisiteStatuses.All(status => status.IsInstalled);
        }
    }

    private void PrerequisiteHelp_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id } || !_prerequisites.TryGetValue(id, out var definition))
        {
            return;
        }

        if (!Uri.TryCreate(definition.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            MessageBox.Show("Tautan bantuan tidak valid.", "DenzGames", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }
}

