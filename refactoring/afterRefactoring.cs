using System;
using System.Collections.Generic;

namespace refactroing
{
    public class afterRefactroring
    {
        public string ProcessOrder(int orderId, List<int> productIds, int customerId, string customerName, string address)
        {
            try
            {
                if (orderId <= 0)
                {
                    return "Invalid order ID";
                }

                if (productIds == null || productIds.Count == 0)
                {
                    return "No products in order";
                }

                if (customerId <= 0)
                {
                    return "Invalid customer ID";
                }

                if (string.IsNullOrEmpty(customerName))
                {
                    return "Customer name is required";
                }

                if (string.IsNullOrEmpty(address))
                {
                    return "Address is required";
                }

                for (int i = 0; i < productIds.Count; i++)
                {
                    for (int j = 0; j < productIds.Count; j++)
                    {
                        if (i != j && productIds[i] == productIds[j])
                        {
                            return "Duplicate products found";
                        }
                    }
                }

                decimal total = 0;
                foreach (var pid in productIds)
                {
                    var price = GetPrice(pid);
                    if (price <= 0)
                    {
                        return $"Invalid price for product {pid}";
                    }
                    total += price;
                }

                SaveOrder(orderId, customerId, total);

                return "Order processed successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private decimal GetPrice(int productId)
        {
            return productId * 10.5m;
        }

        private void SaveOrder(int orderId, int customerId, decimal total)
        {
            Console.WriteLine($"Order {orderId} saved for customer {customerId} with total {total}");
        }

        public string GetOrderInfo(int orderId)
        {
            if (orderId > 0)
            {
                return $"Order {orderId} info";
            }
            return "Invalid order ID";
        }
    }
}