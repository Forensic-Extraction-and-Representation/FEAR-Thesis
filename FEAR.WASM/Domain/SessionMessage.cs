using FEAR.Domain.Agents;
using MudBlazor;

namespace FEAR.WASM.Domain
{
    public enum SessionMessageType
    {
        UserQuery,
        UserChat,
        AgentResponse
    }

    public enum SessionMessageState
    {
        PENDING,
        COMPLETED,
        FAILED
    }

    public class SessionMessageButtonTagsConstants
    {
        public const string SAVED_QUERY = "saved-query";
        public const string RERUN_QUERY = "rerun-query";
        public const string SHARE_QUERY = "share-query";
        public const string GRAPH_INCLUDED = "graph-included";
        public const string RUN_QUERY = "run-bot-query";
    }

    public class SessionMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public SessionMessageType Type { get; set; }
        public string Content { get; set; } = "";
        public SessionMessageState State { get; set; } = SessionMessageState.PENDING;
        public HashSet<string> ButtonTags { get; set; } = new HashSet<string>();

        /// <summary>
        /// For multi-agent responses, contains the individual agent contributions.
        /// If null or empty, the Content property is used instead.
        /// </summary>
        public List<AgentMessage> AgentMessages { get; set; } = new List<AgentMessage>();

        public string Icon => Type switch
        {
            SessionMessageType.UserQuery => Icons.Material.Outlined.Code,
            SessionMessageType.UserChat => Icons.Material.Outlined.Chat,
            SessionMessageType.AgentResponse => Icons.Material.Outlined.DeveloperBoard,
            _ => "bi bi-question-circle"
        };

        public string TypeClass => Type switch
        {
            SessionMessageType.UserQuery => "user-query",
            SessionMessageType.UserChat => "user-chat",
            SessionMessageType.AgentResponse => "bot-response",
            _ => "text-muted"
        };
        public string Timestamp => DateTime.Now.ToString("HH:mm:ss");

        internal void Update(SessionMessage message)
        {
            if (message?.Id == Id)
            {
                Content = message.Content;
                State = message.State;
                ButtonTags = message.ButtonTags ?? new HashSet<string>();
                AgentMessages = message.AgentMessages ?? new List<AgentMessage>();
            }
        }
    }
}