using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services.Voice;
using Microsoft.Maui.Controls;

namespace Calendario_Estudiantil.Views;

/// <summary>
/// Página principal que actúa como contenedor de tabs.
/// Contiene una BottomNavView fija y un ContentArea donde se
/// intercambia el contenido de cada tab (Schedule, Tasks, Subjects).
///
/// Las tab pages (SchedulePage, TasksPage, SubjectsPage) son ContentPages.
/// Como ContentView.Content solo acepta View (no Page), extraemos el
/// Content de cada página y lo asignamos al ContentArea al cambiar de tab.
/// Cada página mantiene su estado en la ViewModel singleton.
/// El LoadDataAsync() se llama manualmente al activar cada tab.
///
/// Además hospeda el FAB global del asistente de voz: graba audio, lo manda
/// a la IA, muestra el resultado y pide confirmación antes de mutar datos.
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>Duración máxima de una grabación antes de cortar sola.</summary>
    private static readonly TimeSpan MaxRecordingDuration = TimeSpan.FromSeconds(60);

    private static readonly Color RecordingColor = Color.FromArgb("#D93025");

    private readonly SchedulePage _schedulePage;
    private readonly TasksPage _tasksPage;
    private readonly SubjectsPage _subjectsPage;
    private readonly IVoiceAssistant _voiceAssistant;
    private readonly IAudioRecorderService _audioRecorder;

    private string _currentTab = "Schedule";
    private bool _isBusy;
    private CancellationTokenSource? _autoStopCts;

    public MainPage(
        SchedulePage schedulePage,
        TasksPage tasksPage,
        SubjectsPage subjectsPage,
        IVoiceAssistant voiceAssistant,
        IAudioRecorderService audioRecorder)
    {
        InitializeComponent();

        _schedulePage = schedulePage;
        _tasksPage = tasksPage;
        _subjectsPage = subjectsPage;
        _voiceAssistant = voiceAssistant;
        _audioRecorder = audioRecorder;

        // Escuchar cambios de tab desde BottomNavView
        BottomNav.TabSelected += OnTabSelected;

        // Mostrar el tab inicial
        ShowTab("Schedule");
    }

    private void OnTabSelected(object? sender, string tabType)
    {
        ShowTab(tabType);
    }

    private void ShowTab(string tabType)
    {
        ContentPage tabPage;
        Task loadTask;

        switch (tabType)
        {
            case "Tasks":
                tabPage = _tasksPage;
                BottomNav.ActiveTab = "Tasks";
                loadTask = _tasksPage.LoadDataAsync();
                break;
            case "Subjects":
                tabPage = _subjectsPage;
                BottomNav.ActiveTab = "Subjects";
                loadTask = _subjectsPage.LoadDataAsync();
                break;
            default: // "Schedule"
                tabPage = _schedulePage;
                BottomNav.ActiveTab = "Schedule";
                loadTask = _schedulePage.LoadDataAsync();
                break;
        }

        _currentTab = tabType;

        // Extraer el View raíz de la página y forzar su BindingContext.
        // Es crítico: al mover el View fuera de la página padre, pierde el
        // BindingContext heredado. Lo seteamos explícitamente para que los
        // bindings ({Binding Propiedad}) sigan funcionando.
        var tabContent = tabPage.Content;
        tabContent.BindingContext = tabPage.BindingContext;
        ContentArea.Content = tabContent;

        // Cargar datos de la tab seleccionada (fire-and-forget)
        _ = loadTask;
    }

    // ==================== Asistente de voz ====================

    private async void OnVoiceFabTapped(object? sender, TappedEventArgs e)
    {
        if (_isBusy)
            return;

        if (_audioRecorder.IsRecording)
            await StopAndProcessAsync();
        else
            await StartListeningAsync();
    }

    private async void OnVoiceCancelClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;

        _autoStopCts?.Cancel();
        await _audioRecorder.CancelAsync();
        HideOverlay();
        ResetFab();
    }

    private async void OnVoiceOverlayTapped(object? sender, TappedEventArgs e)
    {
        if (_isBusy || !_audioRecorder.IsRecording)
            return;

        await StopAndProcessAsync();
    }

    private async Task StartListeningAsync()
    {
        try
        {
            await _audioRecorder.StartAsync();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
            return;
        }

        ShowListeningOverlay();

        _autoStopCts?.Cancel();
        _autoStopCts = new CancellationTokenSource();
        _ = AutoStopAsync(_autoStopCts.Token);
    }

    private async Task AutoStopAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(MaxRecordingDuration, ct);
            if (!ct.IsCancellationRequested && _audioRecorder.IsRecording)
                await MainThread.InvokeOnMainThreadAsync(StopAndProcessAsync);
        }
        catch (OperationCanceledException)
        {
            // Cancelado por el usuario: nada que hacer.
        }
    }

    private async Task StopAndProcessAsync()
    {
        if (_isBusy)
            return;

        _isBusy = true;
        _autoStopCts?.Cancel();
        ShowProcessingOverlay();

        try
        {
            var stream = await _audioRecorder.StopAsync();
            if (stream is null)
            {
                HideOverlay();
                ResetFab();
                return;
            }

            VoiceAssistantResult result;
            using (stream)
            {
                result = await _voiceAssistant.ProcessAudioAsync(stream);
            }

            HideOverlay();
            ResetFab();
            await HandleResultAsync(result);
        }
        catch (Exception ex)
        {
            HideOverlay();
            ResetFab();
            await ShowErrorAsync(ex);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task HandleResultAsync(VoiceAssistantResult result)
    {
        bool mutated = false;

        if (!string.IsNullOrWhiteSpace(result.AssistantText))
            await DisplayAlert("Asistente", result.AssistantText, "OK");

        var messages = result.Results
            .Where(r => !string.IsNullOrWhiteSpace(r.Message))
            .Select(r => r.Message!)
            .ToList();

        if (messages.Count > 0)
        {
            mutated = result.Results.Any(r => r.Success);
            await DisplayAlert("Asistente", string.Join("\n\n", messages), "OK");
        }

        if (result.PendingConfirmations.Count > 0)
        {
            var confirmed = new List<PendingConfirmation>();

            foreach (var pending in result.PendingConfirmations)
            {
                bool accepted = await DisplayAlert("Confirmar", pending.Prompt, "Confirmar", "Cancelar");
                if (accepted)
                    confirmed.Add(pending);
            }

            if (confirmed.Count > 0)
            {
                var confirmedResult = await _voiceAssistant.ConfirmAsync(confirmed);

                var confirmedMessages = confirmedResult.Results
                    .Where(r => !string.IsNullOrWhiteSpace(r.Message))
                    .Select(r => r.Message!)
                    .ToList();

                if (confirmedMessages.Count > 0)
                    await DisplayAlert("Asistente", string.Join("\n\n", confirmedMessages), "OK");

                mutated = mutated || confirmedResult.Results.Any(r => r.Success);
            }
        }

        if (mutated)
            await RefreshCurrentTabAsync();
    }

    private async Task RefreshCurrentTabAsync()
    {
        try
        {
            switch (_currentTab)
            {
                case "Tasks":
                    await _tasksPage.LoadDataAsync();
                    break;
                case "Subjects":
                    await _subjectsPage.LoadDataAsync();
                    break;
                default:
                    await _schedulePage.LoadDataAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refrescando tab tras comando de voz: {ex.Message}");
        }
    }

    private Task ShowErrorAsync(Exception ex)
    {
        // Logueo el error completo: sin esto la captura de pantalla no alcanza para diagnosticar.
        System.Diagnostics.Debug.WriteLine($"[VoiceAssistant] {ex}");

        string message = ex switch
        {
            InvalidOperationException inv when inv.Message.Contains("API key", StringComparison.Ordinal) =>
                "Falta configurar la API key de Gemini.\n\n" +
                "Andá a Configuración → Asistente de voz y pegá tu key.",
            GeminiRateLimitException =>
                "Se alcanzó el límite gratuito de Gemini. Esperá un minuto y volvé a intentar.",
            TimeoutException =>
                "Gemini tardó demasiado en responder. Revisá tu conexión e intentá de nuevo.",
            GeminiRequestException gem =>
                // Muestro el código HTTP y el cuerpo que devolvió Google: es el síntoma que
                // permite distinguir un modelo no disponible (404) de un problema de key/JSON.
                $"{Truncate(gem.Message, 500)}",
            HttpRequestException =>
                "No pude conectarme al servicio de IA. Revisá tu conexión a internet.",
            IOException =>
                "Se cortó la conexión con el servicio de IA. Revisá tu conexión e intentá de nuevo.",
            Exception transport when IsTransportFailure(transport) =>
                // SocketException no hereda de IOException, así que sin este brazo los fallos
                // de transporte crudos ("Socket closed", "Connection reset") llegan sin traducir.
                "Se cortó la conexión con el servicio de IA. Revisá tu conexión e intentá de nuevo.",
            InvalidOperationException inv =>
                inv.Message,
            _ =>
                $"Ocurrió un error con el asistente de voz: {ex.Message}"
        };

        return DisplayAlert("Asistente de voz", message, "OK");
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
            return value;

        return value[..max] + "…";
    }

    private static bool IsTransportFailure(Exception ex)
    {
        if (ex is System.Net.Sockets.SocketException)
            return true;

        string haystack = $"{ex.GetType().Name} {ex.Message}";

        return haystack.Contains("Socket", StringComparison.OrdinalIgnoreCase)
            || haystack.Contains("Connection reset", StringComparison.OrdinalIgnoreCase)
            || haystack.Contains("broken pipe", StringComparison.OrdinalIgnoreCase)
            || haystack.Contains("Connection refused", StringComparison.OrdinalIgnoreCase)
            || haystack.Contains("Unexpected end", StringComparison.OrdinalIgnoreCase);
    }

    // ==================== Overlay ====================

    private void ShowListeningOverlay()
    {
        VoiceOverlayTitle.Text = "Escuchando...";
        VoiceOverlayHint.Text = "Hablá y tocá el micrófono o la pantalla para terminar.";
        VoiceOverlaySpinner.IsVisible = false;
        VoiceOverlaySpinner.IsRunning = false;
        VoiceOverlayCancelButton.IsEnabled = true;
        VoiceOverlay.IsVisible = true;
        VoiceFab.BackgroundColor = RecordingColor;
    }

    private void ShowProcessingOverlay()
    {
        VoiceOverlayTitle.Text = "Procesando...";
        VoiceOverlayHint.Text = "Estoy interpretando lo que dijiste.";
        VoiceOverlaySpinner.IsVisible = true;
        VoiceOverlaySpinner.IsRunning = true;
        VoiceOverlayCancelButton.IsEnabled = false;
        VoiceOverlay.IsVisible = true;
        VoiceFab.BackgroundColor = RecordingColor;
    }

    private void HideOverlay()
    {
        VoiceOverlaySpinner.IsRunning = false;
        VoiceOverlaySpinner.IsVisible = false;
        VoiceOverlay.IsVisible = false;
    }

    private void ResetFab()
    {
        VoiceFab.BackgroundColor = AppConstants.Accent;
    }
}
