namespace Utilities
{
    public static class SD
    {
        public const string AdminRole = "Admin";
        public const string ExecutedOrderRole = "ExecutedOrder";
        public const string CustomerRole = "Customer";
        public const string DeliveryRole = "Delivery";

        public static readonly List<string> ValidRoles = new()
        {
            AdminRole,
            ExecutedOrderRole,
            CustomerRole,
            DeliveryRole
        };
    }
}
