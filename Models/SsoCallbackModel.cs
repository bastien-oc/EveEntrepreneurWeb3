namespace EveEntrepreneurWebPersistency3.Models
{
    public class SsoCallbackModel
    {
        public string TokenUuid { get; set; }
        public string SsoCode   { get; set; }
        public string State     { get; set; }
    }
}
