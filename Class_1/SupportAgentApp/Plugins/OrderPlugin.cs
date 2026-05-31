using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SupportAgentApp.Plugins;

public class OrderPlugin
{
    [KernelFunction]
    [Description("Retrieves the delivery status and details of a customer order using an Order ID.")]
    public string GetOrderStatus([Description("The unique alphanumeric Order ID (e.g., ORD123)")] string orderId)
    {
        return orderId.ToUpper() switch
        {
            "ORD123" => "Order ORD123 is currently 'In Transit'. Expected delivery: Tomorrow by 5:00 PM via DHL.",
            "ORD456" => "Order ORD456 is 'Delivered'. Signed for by Robinson on May 28, 2026.",
            _ => $"Order {orderId} was not found in our system database."
        };
    }
}
