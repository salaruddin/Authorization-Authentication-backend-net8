namespace backend_net8.Core.Entities
{
    public class Message : BaseEntity<long>
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Text { get; set; }
    }
}
