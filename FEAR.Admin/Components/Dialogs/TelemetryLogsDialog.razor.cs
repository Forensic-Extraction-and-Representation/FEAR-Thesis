using FEAR.Admin.Services.Management;
using FEAR.Domain.Telemetry;
using FEAR.Host.Domain.Api.Investigation;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FEAR.Admin.Components.Dialogs
{
    public partial class TelemetryLogsDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
        [Parameter] public Guid InvestigationId { get; set; }
        [Parameter] public string InvestigationName { get; set; }

        private const int MaxSignals = 2000;

        private List<QueryInvestigationTelemetry.TelemetrySignalDto> _signals = new();
        private DateTime? _lastFetch = null;
        private Timer? _timer;
        private bool _isLoading;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private readonly int _refreshInterval = 2000;

        private string _lastFetchDisplay => _lastFetch?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "n/a";

        protected override async Task OnInitializedAsync()
        {
            await Refresh();
            StartPolling();
        }

        private void StartPolling()
        {
            _timer = new Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    await Refresh();
                    StateHasChanged();
                });
            }, null, _refreshInterval, _refreshInterval);
        }

        private async Task Refresh()
        {
            _isLoading = true;
            _hasError = false;
            try
            {
                var req = new QueryInvestigationTelemetry.Request
                {
                    InvestigationName = InvestigationName,
                    InvestigationId = InvestigationId,
                    Since = _lastFetch,
                    Count = 200 // fetch a larger batch for fewer round-trips
                };

                var resp = await InvestigationService.QueryInvestigationTelemetry(req);

                if (_lastFetch == null)
                {
                    _signals = resp.Signals.OrderByDescending(s => s.Timestamp).Take(MaxSignals).ToList();
                }
                else
                {
                    var newOnes = resp.Signals
                        .Where(s => s.Timestamp > _lastFetch.Value)
                        .OrderByDescending(s => s.Timestamp)
                        .ToList();

                    if (newOnes.Count > 0)
                    {
                        _signals.InsertRange(0, newOnes);
                        if (_signals.Count > MaxSignals)
                            _signals = _signals.Take(MaxSignals).ToList();
                    }
                }

                _lastFetch = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _hasError = true;
                _errorMessage = $"Failed to retrieve telemetry: {ex.Message}";
            }
            finally
            {
                _isLoading = false;
            }
        }

        private RenderFragment<QueryInvestigationTelemetry.TelemetrySignalDto> RenderRow => context => builder =>
        {
            var color = GetColorFor(context.SignalType);
            var icon = GetIconFor(context.SignalType);
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "d-flex pa-2");
            builder.AddAttribute(2, "style", "border-bottom: 1px solid var(--mud-palette-divider);");
            builder.OpenComponent<MudIcon>(3);
            builder.AddAttribute(4, "Icon", icon);
            builder.AddAttribute(5, "Color", color);
            builder.AddAttribute(6, "Size", Size.Small);
            builder.CloseComponent();
            builder.OpenElement(7, "div");
            builder.AddAttribute(8, "class", "d-flex flex-column ml-2");
            builder.OpenElement(9, "div");
            builder.AddAttribute(10, "class", "d-flex align-center gap-2");
            builder.OpenComponent<MudChip<string>>(11);
            builder.AddAttribute(12, "T", typeof(string));
            builder.AddAttribute(13, "Size", Size.Small);
            builder.AddAttribute(14, "Color", color);
            builder.AddContent(15, context.SignalType);
            builder.CloseComponent();
            builder.OpenComponent<MudText>(16);
            builder.AddAttribute(17, "Typo", Typo.caption);
            builder.AddAttribute(18, "Color", Color.Secondary);
            builder.AddContent(19, context.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            builder.CloseComponent();
            builder.CloseElement(); // row
            builder.OpenComponent<MudText>(20);
            builder.AddAttribute(21, "Typo", Typo.body2);
            builder.AddAttribute(22, "Class", "fw-bold");
            builder.AddContent(23, context.SignalSource);
            builder.CloseComponent();
            builder.OpenComponent<MudText>(24);
            builder.AddAttribute(25, "Typo", Typo.body2);
            builder.AddAttribute(26, "Style", "white-space:pre-wrap; word-break:break-word;");
            builder.AddContent(27, context.SignalData);
            builder.CloseComponent();
            builder.CloseElement(); // column
            builder.CloseElement(); // row container
        };

        private string GetIconFor(FEARTelemetrySignalTypeEnum type) => type switch
        {
            FEARTelemetrySignalTypeEnum.Exception => Icons.Material.Filled.Error,
            FEARTelemetrySignalTypeEnum.Compilations => Icons.Material.Filled.Warning,
            FEARTelemetrySignalTypeEnum.Dependency => Icons.Material.Filled.Info,
            FEARTelemetrySignalTypeEnum.Environment => Icons.Material.Filled.BugReport,
            FEARTelemetrySignalTypeEnum.Log => Icons.Material.Filled.List,
            FEARTelemetrySignalTypeEnum.Metric => Icons.Material.Filled.ShowChart,
            FEARTelemetrySignalTypeEnum.Trace => Icons.Material.Filled.LogoDev,
            FEARTelemetrySignalTypeEnum.Unknown => Icons.Material.Filled.Help,
            _ => Icons.Material.Filled.Circle
        };

        private Color GetColorFor(FEARTelemetrySignalTypeEnum type) => type switch
        {
            FEARTelemetrySignalTypeEnum.Exception => Color.Error,
            FEARTelemetrySignalTypeEnum.Compilations => Color.Warning,
            FEARTelemetrySignalTypeEnum.Dependency => Color.Info,
            FEARTelemetrySignalTypeEnum.Environment => Color.Secondary,
            FEARTelemetrySignalTypeEnum.Log => Color.Primary,
            FEARTelemetrySignalTypeEnum.Metric => Color.Success,
            FEARTelemetrySignalTypeEnum.Trace => Color.Tertiary,
            FEARTelemetrySignalTypeEnum.Unknown => Color.Dark,
            _ => Color.Default
        };

        private void Close() => MudDialog?.Close();

        public void Dispose() => _timer?.Dispose();
    }
}