using BookHaven.API.Models.Dto.Order;
using BookHaven.API.Models.Dto.Payment;
using BookHaven.API.Services;
using BookHaven.API.Services.IServices;
using BookHaven.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHaven.API.Controllers;

[Route("api/orders")]
[ApiController]
[Authorize(Policy = "Customer")]
public class OrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private string? UserId => User.FindFirst("sub")?.Value; 

    public OrdersController(IOrderService orderService, IPaymentService paymentService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
    }


    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (UserId is null)
            return Unauthorized();
        var result = await _orderService.CreateOrderFromCartAsync(UserId, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return CreatedAtRoute("GetOrder", new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(CancellationToken cancellationToken = default)
    {
        if (UserId is null)
            return Unauthorized();
        var result = await _orderService.GetOrdersByUserIdAsync(UserId, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpGet("{id:int}", Name = "GetOrder")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto?>> GetOrder(int id, CancellationToken cancellationToken = default)
    {
        if (UserId is null)
            return Unauthorized();
        var result = await _orderService.GetOrderByUserIdAsync(id, UserId, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> Cancel(int id, CancellationToken cancellationToken = default)
    {
        if (UserId is null)
            return Unauthorized();
        var result = await _orderService.CancelOrderAsync(id, UserId, cancellationToken: cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpPost("{id:int}/checkout")]
    [ProducesResponseType(typeof(CheckoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CheckoutSessionDto>> Checkout(int id, CancellationToken cancellationToken = default)
    {
        if (UserId is null)
            return Unauthorized();
        var result = await _paymentService.CreateCheckoutSessionAsync(id, UserId, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpGet("admin/all")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders([FromQuery] SD.OrderStatus? status, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetAllOrdersAsync(status, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpPost("admin/{id:int}/process")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> Process(int id, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.ProcessOrderAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpPost("admin/{id:int}/ship")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> Ship(int id, [FromBody] ShipOrderDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.ShipOrderAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpPost("admin/{id:int}/deliver")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> Deliver(int id, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.DeliverOrderAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpPost("admin/{id:int}/cancel")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> AdminCancel(int id, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.AdminCancelOrderAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }
}