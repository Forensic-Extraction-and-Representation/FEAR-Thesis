namespace FEAR.Host.Core.Investigation.Configuration
{
    public delegate void OnInvestigationConfigurationChangedEventHandler(object sender, OnInvestigationConfigurationChangeEventArgs e);
    public class OnInvestigationConfigurationChangeEventArgs : EventArgs
    {
        public string CaseName { get; set; }
        public OnInvestigationConfigurationChangeEventArgs(string caseName)
        {
            CaseName = caseName;
        }
    }
}
