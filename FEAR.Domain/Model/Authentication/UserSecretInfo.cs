namespace FEAR.Domain.Model.Authentication
{
    public class UserSecretInfo
    {
        public int Iterations { get; set; } = 50000;
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string MFASecret { get; set; }
    }
}
