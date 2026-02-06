using System;
using System.Collections.Generic;
using System.Linq;

namespace refactroing
{
    
    /// Исключение, возникающее при ошибке обработки заказа
    
    public class OrderProcessingException : Exception
    {
        public OrderProcessingException(string message) : base(message)
        {
        }

        public OrderProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    
    /// Идентификатор заказа
    
    public readonly struct OrderId
    {
        private readonly int _value;

        public OrderId(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Order ID должен быть положительным", nameof(value));
            }

            _value = value;
        }

        public int Value => _value;

        public override string ToString() => _value.ToString();
    }

    
    /// Идентификатор клиента
    
    public readonly struct CustomerId
    {
        private readonly int _value;

        public CustomerId(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Customer ID должен быть положительным", nameof(value));
            }

            _value = value;
        }

        public int Value => _value;

        public override string ToString() => _value.ToString();
    }

    
    /// Идентификатор продукта
    
    public readonly struct ProductId
    {
        private readonly int _value;

        public ProductId(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Product ID должен быть положительным", nameof(value));
            }

            _value = value;
        }

        public int Value => _value;

        public override string ToString() => _value.ToString();
    }

    
    /// Имя клиента
    
    public readonly struct CustomerName
    {
        private readonly string _value;

        public CustomerName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Customer name не может быть пустым", nameof(value));
            }

            if (value.Length > 100)
            {
                throw new ArgumentException("Customer name слишком длинное", nameof(value));
            }

            _value = value.Trim();
        }

        public string Value => _value;

        public override string ToString() => _value;
    }

    
    /// Адрес доставки
    
    public readonly struct DeliveryAddress
    {
        private readonly string _value;

        public DeliveryAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Адрес доставки не может быть пустым", nameof(value));
            }

            if (value.Length > 200)
            {
                throw new ArgumentException("Адрес доставки слишком длинный", nameof(value));
            }

            _value = value.Trim();
        }

        public string Value => _value;

        public override string ToString() => _value;
    }

    
    /// Детали заказа
    
    public sealed class OrderDetails
    {
        private readonly OrderId _orderId;
        private readonly CustomerId _customerId;
        private readonly CustomerName _customerName;
        private readonly DeliveryAddress _deliveryAddress;
        private readonly IReadOnlyList<ProductId> _productIds;

        public OrderDetails(
            OrderId orderId,
            CustomerId customerId,
            CustomerName customerName,
            DeliveryAddress deliveryAddress,
            IReadOnlyList<ProductId> productIds)
        {
            _orderId = orderId;
            _customerId = customerId;
            _customerName = customerName;
            _deliveryAddress = deliveryAddress;
            _productIds = productIds ?? throw new ArgumentNullException(nameof(productIds));

            Validate();
        }

        public OrderId OrderId => _orderId;
        public CustomerId CustomerId => _customerId;
        public CustomerName CustomerName => _customerName;
        public DeliveryAddress DeliveryAddress => _deliveryAddress;
        public IReadOnlyList<ProductId> ProductIds => _productIds;

        private void Validate()
        {
            if (_productIds.Count == 0)
            {
                throw new OrderProcessingException("Заказ должен содержать как минимум один товар.\r\n");
            }

            if (HasDuplicateProducts())
            {
                throw new OrderProcessingException("В заказе есть дубликаты товаров");
            }
        }

        private bool HasDuplicateProducts()
        {
            var productSet = new HashSet<ProductId>();

            foreach (var productId in _productIds)
            {
                if (!productSet.Add(productId))
                {
                    return true;
                }
            }

            return false;
        }
    }

    
    /// Интерфейс репозитория для работы с продуктами
    
    public interface IProductRepository
    {
        decimal GetProductPrice(ProductId productId);
    }

    
    /// Интерфейс репозитория для работы с заказами
    
    public interface IOrderRepository
    {
        void SaveOrder(OrderId orderId, CustomerId customerId, decimal totalAmount);
        string GetOrderInformation(OrderId orderId);
    }

    
    /// Обработчик заказов
    
    public sealed class OrderProcessor
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderProcessor(
            IProductRepository productRepository,
            IOrderRepository orderRepository)
        {
            _productRepository = productRepository
                ?? throw new ArgumentNullException(nameof(productRepository));
            _orderRepository = orderRepository
                ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        
    /// Обрабатывает заказ
    public void ProcessOrder(OrderDetails orderDetails)
    {
        if (orderDetails == null)
        {
            throw new ArgumentNullException(nameof(orderDetails));
        }

        try
        {
            decimal totalAmount = CalculateTotalAmount(orderDetails.ProductIds);
            SaveOrder(orderDetails.OrderId, orderDetails.CustomerId, totalAmount);
        }
        catch (Exception exception) 
        {
            if (!(exception is OrderProcessingException))
            {
                throw new OrderProcessingException($"Не удалось обработать заказ {orderDetails.OrderId}", exception);
            }
            throw; 
        }
    }

        /// Получает информацию о заказе
        public string GetOrderInformation(OrderId orderId)
        {
            return _orderRepository.GetOrderInformation(orderId);
        }

        private decimal CalculateTotalAmount(IReadOnlyList<ProductId> productIds)
        {
            decimal total = 0;

            foreach (var productId in productIds)
            {
                decimal price = _productRepository.GetProductPrice(productId);

                if (price <= 0)
                {
                    throw new OrderProcessingException($"Неверная цена товара {productId}");
                }

                total += price;
            }

            return total;
        }

        private void SaveOrder(OrderId orderId, CustomerId customerId, decimal totalAmount)
        {
            _orderRepository.SaveOrder(orderId, customerId, totalAmount);
        }
    }

    
    /// Реализация репозитория продуктов
    public sealed class ProductRepository : IProductRepository
    {
        public decimal GetProductPrice(ProductId productId)
        {
            return productId.Value * 10.5m;
        }
    }

    
    /// Реализация репозитория заказов
    
    public sealed class OrderRepository : IOrderRepository
    {
        public void SaveOrder(OrderId orderId, CustomerId customerId, decimal totalAmount)
        {
            Console.WriteLine($"Заказ {orderId} сохранено для клиента {customerId} с общим {totalAmount}");
        }

        public string GetOrderInformation(OrderId orderId)
        {
            return $"Заказ {orderId} информация";
        }
    }

    
    /// Фабрика для создания обработчика заказов
    public static class OrderProcessorFactory
    {
        public static OrderProcessor Create()
        {
            var productRepository = new ProductRepository();
            var orderRepository = new OrderRepository();

            return new OrderProcessor(productRepository, orderRepository);
        }
    }
}