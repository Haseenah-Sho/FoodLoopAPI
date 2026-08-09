using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetMyNotifications
    {
        public record GetMyNotificationsQuery(Guid UserId)
            : IRequest<BaseResponse<ICollection<NotificationResponse>>>;

        public record NotificationResponse(
            Guid Id,
            string Title,
            string MessageContent,
            string NotificationType,
            bool IsRead,
            DateTime DateCreated);

        public class GetMyNotificationsHandler(
            INotificationRepository notificationRepository)
            : IRequestHandler<GetMyNotificationsQuery, BaseResponse<ICollection<NotificationResponse>>>
        {
            public async Task<BaseResponse<ICollection<NotificationResponse>>> Handle(
                GetMyNotificationsQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var notifications = await notificationRepository.GetByUserAsync(request.UserId);

                    var response = notifications.Select(n => new NotificationResponse(
                        n.Id,
                        n.Title,
                        n.MessageContent,
                        n.NotificationType.ToString(),
                        n.IsRead,
                        n.DateCreated)).ToList();

                    return BaseResponse<ICollection<NotificationResponse>>.Success(
                        "Notifications retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<ICollection<NotificationResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}