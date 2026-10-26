namespace tp2.Models
{
    public static class OrderStatuses
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";

        public static readonly string[] All =
        {
            Pending, Processing, Shipped, Delivered, Cancelled
        };

        public static string GetLabel(string? status) => status switch
        {
            Processing => "En préparation",
            Shipped => "Expédiée",
            Delivered => "Livrée",
            Cancelled => "Annulée",
            _ => "En attente"
        };

        public static string GetBadgeClass(string? status) => status switch
        {
            Processing => "status-processing",
            Shipped => "status-shipped",
            Delivered => "status-delivered",
            Cancelled => "status-cancelled",
            _ => "status-pending"
        };
    }
}
