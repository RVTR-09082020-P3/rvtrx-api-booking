using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RVTR.Booking.ObjectModel.Models
{
  public class GuestModel : IValidatableObject
  {
    public int Id { get; set; }
    public int? BookingId { get; set; }
    [Required]
    public virtual BookingModel Booking { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (Booking == null)
      {
        yield return new ValidationResult("Booking cannot be null.");
      }
    }
  }
}
