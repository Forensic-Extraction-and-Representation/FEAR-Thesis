using EventAggregator.Blazor;
using FEAR.WASM.Interactions;
using Microsoft.AspNetCore.Components.Web;

namespace FEAR.WASM.Components.Omnibar
{
    public partial class Omnibar : IHandle<ToolbarEvent<string>>, IHandle<GenericUIEvent>, IHandle<UpdateQueryEvent>
    {
        private class OmnibarUIState
        {
            public string OmnibarStatusText = ". . .";
            public bool OmnibarContentVisible = false;
            public string RequestType = "query"; // or "chat"
            public string BodyWrapperDisplayClass => (OmnibarContentVisible ? "d-flex" : "d-none");
        }

        private OmnibarUIState _uiState = new OmnibarUIState();

        private string ChatText { get; set; } = string.Empty;
        private string QueryText { get; set; } = string.Empty;

        private async Task SendChatAsync()
        {
            var messageText = ChatText.Trim();
            if (string.IsNullOrEmpty(messageText))
                return;

            // Simulate chat service call (replace with your actual service)
            Mediator.PublishAsync(new SendSessionEvent("chat", messageText));
            ChatText = string.Empty;
        }

        private async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && e.CtrlKey)
            {
                await SendChatAsync();
            }
        }

        private async Task SendQueryAsync()
        {
            var messageText = QueryText.Trim();
            if (string.IsNullOrEmpty(messageText))
                return;

            QueryText = string.Empty;

            // Simulate chat service call (replace with your actual service)
            Mediator.PublishAsync(new SendSessionEvent("query", messageText));
        }

        private void ToggleOmnibar(bool? state = null)
        {
            _uiState.OmnibarContentVisible = state ?? !_uiState.OmnibarContentVisible;
        }

        public async Task HandleAsync(ToolbarEvent<string> notification)
        {
            if (notification.Option == "RequestType" && _uiState.RequestType != notification.Value)
            {
                _uiState.RequestType = notification.Value;
                StateHasChanged();
            }
        }

        public async Task HandleAsync(UpdateQueryEvent message)
        {
            QueryText = message.QueryText;
        }

        public async Task HandleAsync(GenericUIEvent notification)
        {
            if (!notification.IsAction(new string[] { GenericUIEventConstants.HIDE_OMNIBAR, GenericUIEventConstants.TOGGLE_OMNIBAR }))
                return;

            bool? state = null;
            if (notification.Action == GenericUIEventConstants.HIDE_OMNIBAR)
                state = false;

            ToggleOmnibar(state);
            StateHasChanged();
        }
    }
}