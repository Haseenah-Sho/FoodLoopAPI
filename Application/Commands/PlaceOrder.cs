using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class PlaceOrder
    {
        public record PlaceOrderItemRequest(Guid ListingId, int Quantity);

        public record PlaceOrderCommand(
            Guid CustomerUserId,
            List<PlaceOrderItemRequest> Items,
            FulfilmentType FulfilmentType,
            string? DeliveryAddress) : IRequest<BaseResponse<PlaceOrderResponse>>;

        public class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
        {
            public PlaceOrderValidator()
            {
                RuleFor(x => x.Items)
                    .NotEmpty().WithMessage("Your order must contain at least one item.");

                RuleForEach(x => x.Items).ChildRules(item =>
                {
                    item.RuleFor(i => i.Quantity)
                        .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
                });

                RuleFor(x => x.DeliveryAddress)
                    .NotEmpty().WithMessage("Delivery address is required for delivery orders.")
                    .When(x => x.FulfilmentType == FulfilmentType.Delivery);
            }
        }

        public class PlaceOrderHandler(
            ICustomerRepository customerRepository,
            IListingRepository listingRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IEmailService emailService,
            IPaystackService paystackService)
            : IRequestHandler<PlaceOrderCommand, BaseResponse<PlaceOrderResponse>>
        {
            public async Task<BaseResponse<PlaceOrderResponse>> Handle(
                PlaceOrderCommand request,
                CancellationToken cancellationToken)
            {
                var decrementedItems = new List<(Guid ListingId, int Quantity)>();

                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<PlaceOrderResponse>.Failure("Customer profile not found.");

                    var requestedListings = new List<(PlaceOrderItemRequest Item, Listing? Listing)>();

                    foreach (var item in request.Items)
                    {
                        var listing = await listingRepository.GetListingAsync(item.ListingId);
                        requestedListings.Add((item, listing));
                    }

                    var validListings = requestedListings
                        .Where(x => x.Listing is not null)
                        .Select(x => x.Listing!)
                        .ToList();

                    var distinctVendorIds = validListings
                        .Select(l => l.VendorId)
                        .Distinct()
                        .ToList();

                    if (distinctVendorIds.Count > 1)
                        return BaseResponse<PlaceOrderResponse>.Failure(
                            "All items in one order must be from the same vendor. Please check out items from different vendors separately.");

                    var vendor = validListings.FirstOrDefault()?.Vendor;

                    var order = new Order
                    {
                        OrderNo = $"FDL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                        CustomerId = customer.Id,
                        FulfilmentType = request.FulfilmentType,
                        DeliveryAddress = request.FulfilmentType == FulfilmentType.Delivery ? request.DeliveryAddress : null,
                        Status = OrderStatus.Confirmed,
                        CreatedBy = customer.UserId.ToString(),
                    };

                    decimal totalAmount = 0;
                    decimal totalDeliveryFee = 0;
                    var fulfilledListingNames = new List<string>();
                    var unfulfilledItems = new List<string>();

                    foreach (var (item, listing) in requestedListings)
                    {
                        if (listing is null)
                        {
                            unfulfilledItems.Add($"Listing {item.ListingId} not found.");
                            continue;
                        }

                        if (listing.Status != ListingStatus.Active)
                        {
                            unfulfilledItems.Add($"{listing.FoodName} is no longer available.");
                            continue;
                        }

                        var stockReserved = await listingRepository.TryDecrementStockAsync(listing.Id, item.Quantity);

                        if (!stockReserved)
                        {
                            unfulfilledItems.Add($"{listing.FoodName} no longer has enough stock available.");
                            continue;
                        }

                        decrementedItems.Add((listing.Id, item.Quantity));

                        order.OrderListings.Add(new OrderListing
                        {
                            ListingId = listing.Id,
                            Quantity = item.Quantity
                        });

                        if (!listing.IsFree)
                            totalAmount += listing.Price!.Value * item.Quantity;

                        if (request.FulfilmentType == FulfilmentType.Delivery)
                            totalDeliveryFee += listing.DeliveryFee;

                        var refreshedListing = await listingRepository.GetListingAsync(listing.Id);
                        if (refreshedListing is not null)
                        {
                            if (refreshedListing.RemainingPortion <= 0)
                            {
                                refreshedListing.Status = ListingStatus.Completed;
                                listingRepository.Update(refreshedListing);
                            }

                            await notificationService.NotifyStockChange(
                                refreshedListing.Id,
                                refreshedListing.RemainingPortion,
                                refreshedListing.Status.ToString());
                        }

                        fulfilledListingNames.Add(listing.FoodName);
                    }

                    if (order.OrderListings.Count == 0)
                    {
                        foreach (var (listingId, quantity) in decrementedItems)
                            await listingRepository.RestoreStockAsync(listingId, quantity);

                        return BaseResponse<PlaceOrderResponse>.Failure(
                            "None of the items in your order could be processed.",
                            unfulfilledItems);
                    }

                    order.TotalAmount = totalAmount + totalDeliveryFee;

                    string? paymentAuthorizationUrl = null;

                    if (order.TotalAmount > 0)
                    {
                        order.Status = OrderStatus.Pending;

                        var paymentReference = $"PAY-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

                        var initResult = await paystackService.InitializeTransactionAsync(
                            customer.User.Email, order.TotalAmount, paymentReference);

                        if (!initResult.Success)
                        {
                            foreach (var (listingId, quantity) in decrementedItems)
                                await listingRepository.RestoreStockAsync(listingId, quantity);

                            return BaseResponse<PlaceOrderResponse>.Failure(
                                $"Payment initialization failed: {initResult.ErrorMessage}");
                        }

                        var payment = new Payment
                        {
                            OrderId = order.Id,
                            UserId = customer.UserId,
                            Amount = order.TotalAmount,
                            Status = PaystackStatus.Pending,
                            PaystackReference = initResult.Reference,
                            CreatedBy = customer.User.Email
                        };

                        order.Payment = payment;
                        paymentAuthorizationUrl = initResult.AuthorizationUrl;
                    }
                    else
                    {
                        order.Status = OrderStatus.Confirmed;
                    }
                    await orderRepository.AddAsync(order);
                    await unitOfWork.SaveAsync();

                    if (vendor is not null)
                    {
                        await notificationService.SendNotificationAsync(
                            vendor.UserId,
                            "New Order Received",
                            $"You have a new order ({order.OrderNo}) from {customer.User.FullName ?? customer.User.UserName}.",
                            NotificationType.NewOrder);

                        try
                        {
                            await emailService.SendOrderStatusEmailAsync(
                                vendor.User.Email,
                                order.OrderNo,
                                "New Order Received",
                                $"You have received a new order ({order.OrderNo}) from {customer.User.FullName ?? customer.User.UserName}.");
                        }
                        catch
                        {
                            // Email failed but order succeeded
                        }
                    }

                    var message = unfulfilledItems.Count > 0
                        ? "Order placed, but some items could not be fulfilled."
                        : "Order placed successfully.";

                    return BaseResponse<PlaceOrderResponse>.Success(
                        message,
                        new PlaceOrderResponse(
                            order.Id,
                            order.OrderNo,
                            order.TotalAmount,
                            order.Status.ToString(),
                            fulfilledListingNames,
                            unfulfilledItems,
                            paymentAuthorizationUrl,
                            order.Payment?.PaystackReference));
                }
                catch (Exception ex)
                {
                    foreach (var (listingId, quantity) in decrementedItems)
                        await listingRepository.RestoreStockAsync(listingId, quantity);

                    return BaseResponse<PlaceOrderResponse>.Failure(
                        $"An error occurred while placing your order: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
        }

        public record PlaceOrderResponse(
            Guid OrderId,
            string OrderNo,
            decimal TotalAmount,
            string Status,
            List<string> FulfilledItems,
            List<string> UnfulfilledItems,
            string? PaymentAuthorizationUrl,
            string? PaymentReference);
    }
}