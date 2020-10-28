
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RVTR.Booking.ObjectModel.Interfaces;
using RVTR.Booking.ObjectModel.Models;

namespace RVTR.Booking.WebApi.Controllers
{
  /// <summary>
  /// Booking controller
  /// </summary>
  [ApiController]
  [ApiVersion("0.0")]
  [EnableCors("Public")]
  [Route("rest/booking/{version:apiVersion}/[controller]")]
  public class BookingController : ControllerBase
  {
    private readonly ILogger<BookingController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Constructor of the booking controller.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="unitOfWork"></param>
    public BookingController(ILogger<BookingController> logger, IUnitOfWork unitOfWork)
    {
      _logger = logger;
      _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Action method for deleting a booking by booking id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
      _logger.LogDebug("Deleting a booking by its ID...");
      await _unitOfWork.Booking.DeleteAsync(id);
      await _unitOfWork.CommitAsync();
      _logger.LogInformation($"Deleted the booking with ID ${id}.");
      return NoContent();
    }

    /// <summary>
    /// Takes in two dates and retrieves bookings between the two dates,
    /// returns all bookings if no checkin/checkout date specified.
    /// </summary>
    /// <param name="checkIn"></param>
    /// <param name="checkOut"></param>
    /// <returns>List of bookings between date range</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookingModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(DateTime? checkIn, DateTime? checkOut)
    {
      _logger.LogDebug("Getting a booking between dates...");
      if (checkIn != null && checkOut != null)
      {
        // Date range sanity check
        if (checkIn > checkOut)
        {
          _logger.LogInformation($"Failed to get bookings - checkIn can't occur after checkOut..");
          return BadRequest();
        }
        _logger.LogInformation($"Retrieved bookings within the given date range ${checkIn} - ${checkOut}.");
        var bookings = await _unitOfWork.Booking.GetBookingsByDatesAsync((DateTime)checkIn, (DateTime)checkOut);
        return Ok(bookings);
      }
      else if (checkIn == null && checkOut == null)
      {
        _logger.LogInformation($"Retrieved all bookings.");
        return Ok(await _unitOfWork.Booking.SelectAsync());
      }
      else
      {
        _logger.LogWarning($"Failed to get bookings - invalid range given..");
        return BadRequest();
      }
    }

    /// <summary>
    /// Action method that returns a single booking by booking id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
      _logger.LogDebug("Getting a booking by booking ID..");
      var booking = await _unitOfWork.Booking.SelectAsync(id);
      if (booking == null)
      {
        _logger.LogWarning($"Booking with ID {id} does not exist.");
        return NotFound(id);
      }
      else
      {
        _logger.LogInformation($"Retrieved the booking with ID {id}.");
        return Ok(booking);
      }
    }

    /// <summary>
    /// Action method that returns a list of bookings associated with an account id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("Account/{id}")]
    [ProducesResponseType(typeof(IEnumerable<BookingModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAccountId(int id)
    {
      _logger.LogDebug("Getting a booking by account ID..");
      var bookings = await _unitOfWork.Booking.GetByAccountId(id);
      return Ok(bookings);
    }

    /// <summary>
    /// Action method that takes in a booking model and adds it into the database.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(BookingModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post(BookingModel booking)
    {
      _logger.LogDebug("Adding a booking...");
      await _unitOfWork.Booking.InsertAsync(booking);
      await _unitOfWork.CommitAsync();
      _logger.LogInformation($"Successfully added the booking {booking}.");
      return CreatedAtAction(
        actionName: nameof(Get),
        routeValues: new { id = booking.Id },
        value: booking
      );
    }

    /// <summary>
    /// Action method that updates a booking resource in the database.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Put(BookingModel booking)
    {
      _logger.LogDebug("Updating a booking...");
      _unitOfWork.Booking.Update(booking);
      await _unitOfWork.CommitAsync();
      _logger.LogDebug($"Successfully updated the booking ${booking}...");
      return NoContent();
    }
  }
}
