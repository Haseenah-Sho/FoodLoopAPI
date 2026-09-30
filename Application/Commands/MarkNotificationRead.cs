using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Commands
{
    public class MarkNotificationRead
    {
        public record MarkNotificationReadCommand(Guid NotificationId)
            : IRequest<BaseResponse<string>>;

        public class MarkNotificationReadHandler(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<MarkNotificationReadCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                MarkNotificationReadCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var notification = await notificationRepository.GetNotificationAsync(request.NotificationId);
                    if (notification is null)
                        return BaseResponse<string>.Failure("Notification not found.");

                    notification.IsRead = true;
                    notification.DateModified = DateTime.UtcNow;

                    notificationRepository.Update(notification);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success("Notification marked as read.", notification.Id.ToString());
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}