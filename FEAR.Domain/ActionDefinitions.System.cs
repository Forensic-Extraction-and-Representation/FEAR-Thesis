using FEAR.Domain.Dto.Authentication;

namespace FEAR.Domain
{
    public static partial class ActionDefinitions
    {
        /// <summary>
        /// System-wide actions for user, role, investigation, permission set, and tool resolver management.
        /// </summary>
        public static class System
        {
            /// <summary>
            /// System-wide actions for managing users, including creation, listing, updating, and deletion.
            /// </summary>
            public static class User
            {
                /// <summary>Action to create a system user.</summary>
                public static readonly SystemAction Create = new SystemAction { Name = "System.User.Create" };
                /// <summary>Action to list system users.</summary>
                public static readonly SystemAction List = new SystemAction { Name = "System.User.List" };
                /// <summary>Action to update a system user.</summary>
                public static readonly SystemAction Update = new SystemAction { Name = "System.User.Update" };
                /// <summary>Action to delete a system user.</summary>
                public static readonly SystemAction Delete = new SystemAction { Name = "System.User.Delete" };
                /// <summary>Action to change a user's password.</summary>
                public static readonly SystemAction ChangePassword = new SystemAction { Name = "System.User.ChangePassword" };

                /// <summary>All user-related system actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { Create, List, Update, Delete, ChangePassword };
            }

            /// <summary>
            /// System-wide actions for managing roles, including creation, listing, updating, and deletion.
            /// </summary>
            public static class Role
            {
                /// <summary>Action to create a system role.</summary>
                public static readonly SystemAction Create = new SystemAction { Name = "System.Role.Create" };
                /// <summary>Action to list system roles.</summary>
                public static readonly SystemAction List = new SystemAction { Name = "System.Role.List" };
                /// <summary>Action to update a system role.</summary>
                public static readonly SystemAction Update = new SystemAction { Name = "System.Role.Update" };
                /// <summary>Action to delete a system role.</summary>
                public static readonly SystemAction Delete = new SystemAction { Name = "System.Role.Delete" };

                /// <summary>All role-related system actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { Create, List, Update, Delete };
            }

            /// <summary>
            /// System-wide actions for managing investigations, including creation, listing, updating, and deletion.
            /// </summary>
            public static class Investigation
            {
                /// <summary>Action to create a new investigation.</summary>
                public static readonly SystemAction Create = new SystemAction { Name = "System.Investigation.Create" };
                /// <summary>Action to list investigations.</summary>
                public static readonly SystemAction List = new SystemAction { Name = "System.Investigation.List" };
                /// <summary>Action to update an existing investigation.</summary>
                public static readonly SystemAction Update = new SystemAction { Name = "System.Investigation.Update" };
                /// <summary>Action to delete an investigation.</summary>
                public static readonly SystemAction Delete = new SystemAction { Name = "System.Investigation.Delete" };

                /// <summary>All investigation-related system actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { Create, List, Update, Delete };
            }

            public static class OpenIdConnect
            {
                /// <summary>Action to manage OpenID Connect settings.</summary>
                public static readonly SystemAction ManageSettings = new SystemAction { Name = "System.OpenIdConnect.ManageSettings" };
                /// <summary>All OpenID Connect-related system actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { ManageSettings };
            }

            public static class ColorMap
            {
                /// <summary>Action to manage color maps.</summary>
                public static readonly SystemAction Manage = new SystemAction { Name = "System.ColorMap.Manage" };
                /// <summary> All color map-related system actions. </summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { Manage };
            }

            /// <summary>
            /// System-wide actions for managing permission sets, including creation, listing, updating, and deletion.
            /// </summary>
            public static class PermissionSet
            {
                /// <summary>Action to create a new permission set.</summary>
                public static readonly SystemAction Create = new SystemAction { Name = "System.PermissionSet.Create" };
                /// <summary>Action to list permission sets.</summary>
                public static readonly SystemAction List = new SystemAction { Name = "System.PermissionSet.List" };
                /// <summary>Action to update an existing permission set.</summary>
                public static readonly SystemAction Update = new SystemAction { Name = "System.PermissionSet.Update" };
                /// <summary>Action to delete a permission set.</summary>
                public static readonly SystemAction Delete = new SystemAction { Name = "System.PermissionSet.Delete" };

                /// <summary>All permission set-related system actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { Create, List, Update, Delete };
            }

            /// <summary>
            /// System-wide actions for managing tool resolvers, which handle the resolution of tools and their configurations.
            /// </summary>
            public static class ToolResolver
            {
                /// <summary>Action to manage tool resolvers.</summary>
                public static readonly SystemAction Manage = new SystemAction { Name = "System.ToolResolver.Manage" };

                /// <summary> All tool resolver-related system actions. </summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { Manage };
            }

            /// <summary>All system-level actions across all system areas.</summary>
            public static readonly ICollection<SystemAction> All = new List<SystemAction>()
                .Union(User.All)
                .Union(Role.All)
                .Union(Investigation.All)
                .Union(PermissionSet.All)
                .Union(ToolResolver.All)
                .Union(OpenIdConnect.All)
                .Union(ColorMap.All)
                .ToList();
        }
    }
}
