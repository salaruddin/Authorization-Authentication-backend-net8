namespace backend_net8.Core.Entities
{
    public class BaseEntity<TID>
    {
        public  TID Id { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.Now;
        public DateTime LastUpdatedAt { get; set;} = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }= false;
    }
}
