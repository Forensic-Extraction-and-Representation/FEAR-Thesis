using FEAR.Domain.Dto.Authentication;

namespace FEAR.Domain
{
    /// <summary>
    /// Provides investigation-specific actions for roles, users, artefacts, evidence, and categories.
    /// These actions are used for permission and authorization checks within the context of an investigation.
    /// </summary>
    public static partial class ActionDefinitions
    {
        /// <summary>
        /// Investigation-specific actions for roles, users, artefacts, evidence, and categories.
        /// </summary>
        public static class Investigation
        {
            /// <summary>
            /// Actions related to roles within an investigation, such as creating, listing, updating, and deleting roles.
            /// </summary>
            public static class Role
            {
                /// <summary>Action to create a role within an investigation.</summary>
                public static readonly SystemAction Create = new SystemAction { Name = "Investigation.Role.Create" };
                /// <summary>Action to list roles within an investigation.</summary>
                public static readonly SystemAction List = new SystemAction { Name = "Investigation.Role.List" };
                /// <summary>Action to update a role within an investigation.</summary>
                public static readonly SystemAction Update = new SystemAction { Name = "Investigation.Role.Update" };
                /// <summary>Action to delete a role within an investigation.</summary>
                public static readonly SystemAction Delete = new SystemAction { Name = "Investigation.Role.Delete" };

                /// <summary>All role-related investigation actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { Create, List, Update, Delete };
            }

            /// <summary>
            /// Actions related to users within an investigation, such as adding/removing roles and listing users.
            /// </summary>
            public static class User
            {
                /// <summary>Action to add a role to a user within an investigation.</summary>
                public static readonly SystemAction AddRole = new SystemAction { Name = "Investigation.User.AddRole" };
                /// <summary>Action to remove a role from a user within an investigation.</summary>
                public static readonly SystemAction RemoveRole = new SystemAction { Name = "Investigation.User.RemoveRole" };
                /// <summary>Action to list users within an investigation.</summary>
                public static readonly SystemAction List = new SystemAction { Name = "Investigation.User.List" };

                /// <summary>All user-related investigation actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { AddRole, RemoveRole, List };
            }

            /// <summary>
            /// Actions related to artefacts within an investigation, such as adding or removing categories.
            /// </summary>
            public static class Artifact
            {
                /// <summary>Action to add a category to an artefact within an investigation.</summary>
                public static readonly SystemAction AddCategory = new SystemAction { Name = "Investigation.Artifact.AddCategory" };
                /// <summary>Action to remove a category from an artefact within an investigation.</summary>
                public static readonly SystemAction RemoveCategory = new SystemAction { Name = "Investigation.Artifact.RemoveCategory" };

                /// <summary>All artefact-related investigation actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { AddCategory, RemoveCategory };
            }

            /// <summary>
            /// Actions related to evidence within an investigation, such as uploading, deleting, or downloading evidence.
            /// </summary>
            public static class Evidence
            {
                /// <summary>Action to upload evidence within an investigation.</summary>
                public static readonly SystemAction Upload = new SystemAction { Name = "Investigation.Evidence.Upload" };
                /// <summary>Action to delete evidence within an investigation.</summary>
                public static readonly SystemAction Delete = new SystemAction { Name = "Investigation.Evidence.Delete" };
                /// <summary>Action to download evidence within an investigation.</summary>
                public static readonly SystemAction Download = new SystemAction { Name = "Investigation.Evidence.Download" };

                /// <summary>All evidence-related investigation actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { Upload, Delete, Download };
            }

            /// <summary>
            /// Actions related to categories within an investigation, such as viewing, listing, updating, or deleting categories.
            /// </summary>
            public static class Category
            {
                /// <summary>Action to view a category within an investigation.</summary>
                public static readonly SystemAction View = new SystemAction { Name = "Investigation.Category.View" };
                /// <summary>Action to list categories within an investigation.</summary>
                public static readonly SystemAction List = new SystemAction { Name = "Investigation.Category.List" };
                /// <summary>Action to update a category within an investigation.</summary>
                public static readonly SystemAction Update = new SystemAction { Name = "Investigation.Category.Update" };
                /// <summary>Action to delete a category within an investigation.</summary>
                public static readonly SystemAction Delete = new SystemAction { Name = "Investigation.Category.Delete" };

                /// <summary>All category-related investigation actions.</summary>
                public static readonly ICollection<SystemAction> All = new List<SystemAction> { View, List, Update, Delete };
            }

            /// <summary>Action to open an investigation.</summary>
            public static readonly SystemAction Open = new SystemAction { Name = "Investigation.Open" };

            /// <summary>All investigation-level actions across all investigation areas.</summary>
            public static readonly ICollection<SystemAction> All = new List<SystemAction>()
                .Union(Role.All)
                .Union(User.All)
                .Union(Artifact.All)
                .Union(Evidence.All)
                .Union(Category.All)
                .ToList();
        }
    }
}
