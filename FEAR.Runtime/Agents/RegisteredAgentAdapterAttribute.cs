using FEAR.Domain.Agents;

namespace FEAR.Runtime.Agents
{
    public class RegisteredAgentAdapterAttribute : Attribute
    {
        public string Name { get; }
        public AgentAdapterType AgentAdapterType { get; }

        public RegisteredAgentAdapterAttribute(string name, AgentAdapterType agentAdapterType)
        {
            Name = name;
            AgentAdapterType = agentAdapterType;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisteredAgentAdapterFieldAttribute : Attribute
    {
        public string FieldName { get; }
        public string Description { get; }
        public bool IsRequired { get; }
        public bool IsSecret { get; }
        public RegisteredAgentAdapterFieldAttribute(string fieldName, string description, bool isRequired = false, bool isSecret = false)
        {
            FieldName = fieldName;
            Description = description;
            IsRequired = isRequired;
            IsSecret = isSecret;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisteredAgentAdapterFieldOptionAttribute : Attribute
    {
        public string FieldName { get; }
        public string DisplayValue { get; }
        public string SelectValue { get; }
        public string Description { get; }
        public RegisteredAgentAdapterFieldOptionAttribute(string fieldName, string displayValue, string selectValue, string description)
        {
            FieldName = fieldName;
            DisplayValue = displayValue;
            SelectValue = selectValue;
            Description = description;
        }
    }
}
