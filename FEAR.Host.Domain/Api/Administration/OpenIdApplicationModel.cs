using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FEAR.Host.Domain.Api.Administration
{
    public class OpenIdApplicationModel : OpenIddict.Abstractions.OpenIddictApplicationDescriptor
    {

        /// <summary>
        /// Gets or sets the application type associated with the application.
        /// </summary>
        public string? ApplicationType { get; set; }

        /// <summary>
        /// Gets or sets the client identifier associated with the application.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret associated with the application.
        /// Note: depending on the application manager used when creating it,
        /// this property may be hashed or encrypted for security reasons.
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the client type associated with the application.
        /// </summary>
        public string? ClientType { get; set; }

        /// <summary>
        /// Gets or sets the consent type associated with the application.
        /// </summary>
        public string? ConsentType { get; set; }

        /// <summary>
        /// Gets or sets the display name associated with the application.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the JSON Web Key Set associated with the application.
        /// </summary>
        public JsonWebKeySet? JsonWebKeySet { get; set; }

        /// <summary>
        /// Gets the localized display names associated with the application.
        /// </summary>
        [JsonPropertyName("displayNames")]
        public Dictionary<CultureInfo, string> DisplayNames { get; set; } = [];

        /// <summary>
        /// Gets the permissions associated with the application.
        /// </summary>
        [JsonPropertyName("permissions")]
        public HashSet<string> Permissions { get; set; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets the post-logout redirect URIs associated with the application.
        /// </summary>
        [JsonPropertyName("postLogOutRedirectUris")]
        public HashSet<Uri> PostLogoutRedirectUris { get; set; } = [];

        /// <summary>
        /// Gets the additional properties associated with the application.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, JsonElement> Properties { get; set; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets the redirect URIs associated with the application.
        /// </summary>
        [JsonPropertyName("redirectUris")]
        public HashSet<Uri> RedirectUris { get; set; } = [];

        /// <summary>
        /// Gets the requirements associated with the application.
        /// </summary>
        [JsonPropertyName("requirements")]
        public HashSet<string> Requirements { get; set; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Gets the settings associated with the application.
        /// </summary>
        [JsonPropertyName("settings")]
        public Dictionary<string, string> Settings { get; set; } = new(StringComparer.Ordinal);
    }
}
