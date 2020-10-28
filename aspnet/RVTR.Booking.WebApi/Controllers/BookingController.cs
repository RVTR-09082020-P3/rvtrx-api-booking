
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
      var booking = await _unitOfWork.Booking.SelectAsync(id);
      if (booking == null)
      {
        _logger.LogInformation($"Could not find booking to delete @ id = {id}.");
        return NotFound(id);
      }
      else
      {
        await _unitOfWork.Booking.DeleteAsync(id);
        await _unitOfWork.CommitAsync();
        _logger.LogInformation($"Succesfully deleted booking @ id = {id}.");
        return NoContent();
      }
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
      if (checkIn != null && checkOut != null)
      {
        // Date range sanity check
        if (checkIn > checkOut || checkIn == checkOut)
        {
          _logger.LogInformation($"Check In Date cannot be later than or equal to Check Out Date.");
          return BadRequest();
        }

        var bookings = await _unitOfWork.Booking.GetBookingsByDatesAsync((DateTime)checkIn, (DateTime)checkOut);
        return Ok(bookings);
      }
      else if (checkIn == null && checkOut == null)
      {
        _logger.LogInformation($"Check In Date and Check Out Date cannot be null.");
        return BadRequest();//// Before my edit, this line read: return Ok(await _unitOfWork.Booking.SelectAsync());
        // Changed the above line because having client side validation that requies check in and Checkout dates would
        // mean that empty checkIn and checkout dates is a bad request.
      }
      else
      {
        return BadRequest();
      }
    }

    /// <summary>
    ///
    /// Action method that returns a single booking by booking id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
      var booking = await _unitOfWork.Booking.SelectAsync(id);
      if (booking == null)
      {
        _logger.LogInformation($"Could not find booking to get @ id = {id}.");
        return NotFound(id);
      }
      else
      {
        _logger.LogInformation($"Succesfully found booking to get @ id = {id}.");
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAccountId(int id)
    {
      var bookings = await _unitOfWork.Booking.GetByAccountId(id);
      if (bookings == null)
      {
        _logger.LogInformation($"Could not find bookings to get @ account id = {id}.");
        return NotFound(id);
      }
      else
      {
        _logger.LogInformation($"Succesfully found bookings to get @ account id = {id}.");
        return Ok(bookings);
      }
    }

    /// <summary>
    /// Action method that takes in a booking model and adds it into the database.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(BookingModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(BookingModel booking)
    {
      var validationResults = booking.Validate(new ValidationContext(booking));
      if (validationResults != null || validationResults.Count() > 0)
      {
        _logger.LogInformation($"Invalid booking '{booking}'.");
        return BadRequest();
      }
      else
      {
        _logger.LogInformation($"Successfully added the booking '{booking}'.");
        await _unitOfWork.Booking.InsertAsync(booking);
        await _unitOfWork.CommitAsync();

        return CreatedAtAction(
          actionName: nameof(Get),
          routeValues: new { id = booking.Id },
          value: booking
        );
      }
    }

    /// <summary>
    /// Action method that updates a booking resource in the database.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Put(BookingModel booking)
    {
      var validationResults = booking.Validate(new ValidationContext(booking));
      if (validationResults != null || validationResults.Count() > 0)
      {
        _logger.LogInformation($"Invalid booking '{booking}'.");
        return BadRequest();
      }
      else
      {
        _logger.LogInformation($"Successfully added the booking '{booking}'.");
        _unitOfWork.Booking.Update(booking);
        await _unitOfWork.CommitAsync();
        return NoContent();
      }
    }
  }
}
