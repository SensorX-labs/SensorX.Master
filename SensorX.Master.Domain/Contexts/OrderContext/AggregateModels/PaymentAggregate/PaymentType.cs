namespace SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels
{
    public enum PaymentType
    {
        All, // thanh toán 1 lần 
        Partial // thanh toán 30% -> thanh toán 70%
    }
}