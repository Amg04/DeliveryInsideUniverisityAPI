// Factory Pattern

public interface IDelivery
{
    void Deliver();
}

public class StandardDelivery : IDelivery
{
    public void Deliver()
    {
        Console.WriteLine("Standard Delivery Processed.");
    }
}

public class ExpressDelivery : IDelivery
{
    public void Deliver()
    {
        Console.WriteLine("Express Delivery Processed.");
    }
}

public enum DeliveryType
{
    Standard,
    Express
}

interface IDeliveryFactory
{
    public IDelivery CreateDelivery(DeliveryType type);
}

public class DeliveryFactory
{
    public IDelivery CreateDelivery(DeliveryType type)
    {
        switch (type)
        {
            case DeliveryType.Standard:
                return new StandardDelivery();
            case DeliveryType.Express:
                return new ExpressDelivery();
            default:
                throw new ArgumentException("Invalid Delivery Type");
        }
    }
}

//  in program 
//DeliveryFactory DF = new DeliveryFactory();
//IDelivery delivery = DF.CreateDelivery(DeliveryType.Standard); // Standard or Express
//delivery.Deliver();