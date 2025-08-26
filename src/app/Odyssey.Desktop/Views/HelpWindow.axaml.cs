// =============================================
// 4. HelpWindow.axaml.cs - Code-behind
// =============================================
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaWebView;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Odyssey.Help
{
    public partial class HelpWindow : Window
    {
        private readonly HelpSystem _helpSystem;
        private string? _currentTopicId;

        public HelpWindow()
        {
            InitializeComponent();
            _helpSystem = HelpSystem.Instance;
            
            // Subscribe to language changes
            _helpSystem.LanguageChanged += OnHelpSystemLanguageChanged;
            
            // Initialize
            LoadTopics();
            SetInitialLanguage();
        }

        private void SetInitialLanguage()
        {
            var currentLang = _helpSystem.CurrentLanguage;
            var comboBox = this.FindControl<ComboBox>("LanguageComboBox");
            
            if (comboBox != null)
            {
                var item = comboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Tag?.ToString() == currentLang);
                
                if (item != null)
                {
                    comboBox.SelectedItem = item;
                }
            }
        }

        private void LoadTopics()
        {
            var listBox = this.FindControl<ListBox>("TopicsListBox");
            if (listBox != null)
            {
                //listBox.Items = _helpSystem.GetAllTopics();
                listBox.ItemsSource = _helpSystem.GetAllTopics();

                // Select first topic by default
                if (listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }
            }
        }

        private async void OnTopicSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is HelpTopic topic)
            {
                await LoadTopicContent(topic.Id);
            }
        }

        private async Task LoadTopicContent(string topicId)
        {
            _currentTopicId = topicId;
            
            var loadingGrid = this.FindControl<Grid>("LoadingGrid");
            var errorGrid = this.FindControl<Grid>("ErrorGrid");
            var webView = this.FindControl<WebView>("ContentWebView");
            
            if (loadingGrid != null && webView != null && errorGrid != null)
            {
                loadingGrid.IsVisible = true;
                errorGrid.IsVisible = false;
                
                try
                {
                    var url = _helpSystem.GetTopicUrl(topicId);
                    /*
                    // Différentes méthodes selon la version d'Avalonia
                    if (webView.Source != null)
                    {
                        // Avalonia 11+ avec Source property
                        webView.Source = new Uri(url);
                    }
                    else*/
                    {
                        // Fallback: utiliser la méthode Navigate si disponible
                        var navigateMethod = webView.GetType().GetMethod("Navigate", new[] { typeof(Uri) });
                        if (navigateMethod != null)
                        {
                            navigateMethod.Invoke(webView, new object[] { new Uri(url) });
                        }
                        else
                        {
                            // Dernière solution: définir la source via réflexion
                            var sourceProperty = webView.GetType().GetProperty("Source");
                            if (sourceProperty != null)
                            {
                                sourceProperty.SetValue(webView, new Uri(url));
                            }
                        }
                    }

                    // Attendre que la page soit chargée
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    ShowError($"Could not load content: {ex.Message}");
                }
                finally
                {
                    loadingGrid.IsVisible = false;
                }
            }
        }

        private void ShowError(string message)
        {
            var errorGrid = this.FindControl<Grid>("ErrorGrid");
            var errorMessage = this.FindControl<TextBlock>("ErrorMessage");
            
            if (errorGrid != null && errorMessage != null)
            {
                errorMessage.Text = message;
                errorGrid.IsVisible = true;
            }
        }

        public async Task NavigateToTopic(string topicId)
        {
            var topic = _helpSystem.GetTopic(topicId);
            if (topic != null)
            {
                var listBox = this.FindControl<ListBox>("TopicsListBox");
                if (listBox != null)
                {
                    listBox.SelectedItem = topic;
                    await LoadTopicContent(topicId);
                }
            }
        }

        private void OnLanguageChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item)
            {
                var language = item.Tag?.ToString();
                if (!string.IsNullOrEmpty(language) && language != _helpSystem.CurrentLanguage)
                {
                    _helpSystem.CurrentLanguage = language;
                }
            }
        }

        private void OnHelpSystemLanguageChanged(string newLanguage)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                LoadTopics();
                
                // Reload current topic in new language
                if (!string.IsNullOrEmpty(_currentTopicId))
                {
                    await LoadTopicContent(_currentTopicId);
                }
            });
        }

        private async void OnRefreshClicked(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentTopicId))
            {
                await LoadTopicContent(_currentTopicId);
            }
        }

        private async void OnRetryClicked(object? sender, RoutedEventArgs e)
        {
            var errorGrid = this.FindControl<Grid>("ErrorGrid");
            if (errorGrid != null)
            {
                errorGrid.IsVisible = false;
            }
            
            if (!string.IsNullOrEmpty(_currentTopicId))
            {
                await LoadTopicContent(_currentTopicId);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _helpSystem.LanguageChanged -= OnHelpSystemLanguageChanged;
            base.OnClosed(e);
        }
    }
}