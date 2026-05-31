namespace AISmartHub.Api.Models
{
    public class AIInteraction
    {
        public int Id { get; set; }
        public string InteractionType { get; set; } = string.Empty;
        public string InputData { get; set; } = string.Empty;
        public string OutputData { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
