using System;

namespace FEAR.Blazor.Shared.Components.Wizards
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class WizardStepAttribute : Attribute
    {
        public string Title { get; }
        public string? Icon { get; }

        public WizardStepAttribute(string title, string? icon = null)
        {
            Title = title;
            Icon = icon;
        }
    }
}
