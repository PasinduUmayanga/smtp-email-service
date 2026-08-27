using Microsoft.AspNetCore.Mvc;
using SmtpEmailService.Application.Features.Emails.SendEmail;

namespace SmtpEmailService.Api.Controllers;

[ApiController]
[Route("api/emails")]
public sealed class EmailsController(SendEmailHandler handler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<SendEmailResult>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<SendEmailResult>> SendAsync([FromBody] SendEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return Accepted($"/api/emails/{result.MessageId}", result);
    }
}
