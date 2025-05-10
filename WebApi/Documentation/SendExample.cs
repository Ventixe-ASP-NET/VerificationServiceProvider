
using Swashbuckle.AspNetCore.Filters;
using WebApi.Models;

namespace WebApi.Documentation;

public class SendExample : IExamplesProvider<SendVerificationCodeRequest>
{
    public SendVerificationCodeRequest GetExamples()
    {
        return new SendVerificationCodeRequest
        {
            Email = "john.smith@mail.com"
        };
    }
}