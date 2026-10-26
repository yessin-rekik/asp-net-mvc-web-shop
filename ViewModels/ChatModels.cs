using System.ComponentModel.DataAnnotations;

namespace tp2.ViewModels
{
    public class ChatRequest
    {
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        public string Reply { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        public List<ChatProductLink> Products { get; set; } = new();
        public List<ChatLink> Links { get; set; } = new();
    }

    public class ChatProductLink
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public float Price { get; set; }
        public string? Image { get; set; }
    }

    public class ChatLink
    {
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
