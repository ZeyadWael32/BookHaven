using BookHaven.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BookHaven.API.Controllers;


[ApiController]
[Route("api/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _config;

    public StripeWebhookController(IPaymentService paymentService, IConfiguration config)
    {
        _paymentService = paymentService;
        _config = config;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync(cancellationToken); // must use raw body not [FromBody] because Stripe sends raw JSON
        var webhookSecret = _config["Stripe:WebhookSecret"];

        if (string.IsNullOrWhiteSpace(webhookSecret))
            throw new InvalidOperationException("Stripe webhook secret is not configured. Please set 'Stripe:WebhookSecret' in the configuration.");

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException)
        {
            return BadRequest();
        }

        await _paymentService.HandleEventAsync(stripeEvent, cancellationToken);
        return Ok();
    }
}
