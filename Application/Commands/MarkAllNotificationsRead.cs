using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Commands
{
    public class MarkAllNotificationsRead
    {
        public record MarkAllNotificationsReadCommand(Guid UserId)
            : IRequest<BaseResponse<string>>;

        public class MarkAllNotificationsReadHandler(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<MarkAllNotificationsReadCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                MarkAllNotificationsReadCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var notifications = await notificationRepository.GetByUserAsync(request.UserId);
                    var unread = notifications.Where(n => !n.IsRead).ToList();

                    foreach (var notification in unread)
                    {
                        notification.IsRead = true;
                        notification.DateModified = DateTime.UtcNow;
                        notificationRepository.Update(notification);
                    }

                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success(
                        $"{unread.Count} notification{(unread.Count != 1 ? "s" : "")} marked as read.",
                        unread.Count.ToString());
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}