using VideoManager.Application.Common;
using VideoManager.Application.Common.Reponse;

namespace VideoManager.Application.Commands.Interfaces;

public interface ITokenCommand
{
    LoginResponse Execute(LoginRequest request);
}
