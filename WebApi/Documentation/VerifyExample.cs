using Swashbuckle.AspNetCore.Filters;
using WebApi.Models;

namespace WebApi.Documentation;

public class VerifyExample : IExamplesProvider<VerifyVerificationCodeRequest>
{
    public VerifyVerificationCodeRequest GetExamples()
    {
        return new VerifyVerificationCodeRequest
        {
            Email = "john.smith@mail.com",
            Code = "123456"
        };
    }
}