using Application.Features.Responses;

namespace Application.Features;

public class BaseCommand<T> : IRequest<BaseResponse<T>>
{
}
