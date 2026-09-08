using FEAR.Blazor.Shared.Interop;
using FEAR.WASM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FEAR.WASM.Components.Dialogs
{
    class ArtifactWorkItemStatus
    {
        public Guid ExecutionId { get; set; }
        public int NotFoundCount { get; set; } = 0;
        public int FoundCount { get; set; } = 0;
    }

    public partial class UploadDialog
    {
        private DotNetObjectReference<UploadDialog> dotNetReference;
        public UploadDialog()
        {
            dotNetReference = DotNetObjectReference.Create(this);
        }

        // Need to have this backed by a local service to allow for state persistence
        [Inject] InvestigationService InvestigationService { get; set; }
        [Inject] TimeoutCallbackInterop TimeoutCallbackInterop { get; set; }

        private const string DefaultDragClass = "relative rounded-lg border-2 border-dashed pa-2 mud-width-full mud-height-full";
        private string _dragClass = DefaultDragClass;
        private List<string> AugmentWith { get; set; } = new();

        private MudFileUpload<IReadOnlyList<IBrowserFile>>? _fileUpload;

        private string _artifactJsonText = string.Empty;
        public string ArtifactCodeText
        {
            get
            {
                return _artifactJsonText;
            }
            set
            {
                _artifactJsonText = value;
                ArtifactData = JsonNode.Parse(value) ?? new JsonObject();
            }
        }

        private void AddAugmentField(string field)
        {
            AugmentWith.Add(field);
            var values = field.Split(field.Contains('=') ? '=' : ':', 2);
            if (values.Length == 2)
            {
                var key = values[0].Trim();
                var value = values[1].Trim();
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    if (ArtifactData is JsonArray)
                    {
                        foreach (var item in ArtifactData.AsArray())
                        {
                            if (item is JsonObject obj)
                            {
                                obj[key] = value;
                            }
                        }
                    }
                    else
                    {
                        ArtifactData[key] = value;
                    }

                    _artifactJsonText = ArtifactData.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                }
            }
        }

        private void DeleteAugmentField(string field)
        {
            AugmentWith.Remove(field);
            var values = field.Split(field.Contains('=') ? '=' : ':', 2);
            if (values.Length == 2)
            {
                var key = values[0].Trim();
                if (!string.IsNullOrEmpty(key))
                {
                    if (ArtifactData is JsonArray)
                    {
                        foreach (var item in ArtifactData.AsArray())
                        {
                            if (item is JsonObject obj && obj.ContainsKey(key))
                            {
                                obj.Remove(key);
                            }
                        }
                    }
                    else if (ArtifactData is JsonObject obj && obj.ContainsKey(key))
                    {
                        obj.Remove(key);
                    }

                    _artifactJsonText = ArtifactData.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                }
            }
        }

        private Task OpenFilePickerAsync()
            => _fileUpload?.OpenFilePickerAsync() ?? Task.CompletedTask;

        private async Task OnInputFileChanged(InputFileChangeEventArgs e)
        {
            ClearDragClass();
            ArtifactData = await JsonNode.ParseAsync(e.File.OpenReadStream(256 * 1024 * 1024));
            _artifactJsonText = ArtifactData.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        private void SetDragClass()
            => _dragClass = $"{DefaultDragClass} mud-border-primary";

        private void ClearDragClass()
            => _dragClass = DefaultDragClass;

        private JsonNode ArtifactData { get; set; } = new JsonObject();
        private string ArtifactResponse { get; set; } = string.Empty;

        Dictionary<Guid, ArtifactWorkItemStatus> executionIds = new Dictionary<Guid, ArtifactWorkItemStatus>();

        private async Task SubmitJsonAsync()
        {
            var response = await InvestigationService.PostArtifactAsync(ArtifactData.ToJsonString());
            executionIds.Add(response.ExecutionId, new ArtifactWorkItemStatus());
            await TimeoutCallbackInterop.SetTimeoutAsync(dotNetReference, nameof(CheckResponse), 5000);
        }

        [JSInvokable]
        public async Task CheckResponse()
        {
            if (executionIds.Count == 0)
                return;
            foreach (var id in executionIds.Keys)
            {
                Tuple<bool, string> response = await InvestigationService.GetArtifactWorkItemAsync(id);
                if (response.Item1)
                {
                    ArtifactResponse = response.Item2;
                    executionIds.Remove(id);
                    StateHasChanged();
                }
                else
                {
                    executionIds[id].NotFoundCount++;
                    if(executionIds[id].NotFoundCount <= 5)
                        await TimeoutCallbackInterop.SetTimeoutAsync(dotNetReference, nameof(CheckResponse), 1000);
                }
            }
        }

    }

}