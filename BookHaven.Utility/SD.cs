namespace BookHaven.Utility;

public static class SD
{
    public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }
    public enum PaymentStatus { Pending, Paid, Failed, Refunded }
}
