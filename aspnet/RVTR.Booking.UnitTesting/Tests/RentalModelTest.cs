using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RVTR.Booking.ObjectModel.Models;
using Xunit;

namespace RVTR.Booking.UnitTesting.Tests
{
  public class RentalModelTest
  {
    public static readonly IEnumerable<object[]> Rentals = new List<object[]>
    {
      new object[]
      {
        new RentalModel()
        {
          Id = 0,
          BookingModelId = 0,
        }
      }
    };

    [Theory]
    [MemberData(nameof(Rentals))]
    public void Test_Create_RentalModel(RentalModel rental)
    {
      var validationContext = new ValidationContext(rental);
      var actual = Validator.TryValidateObject(rental, validationContext, null, true);

      Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(Rentals))]
    public void Test_Validate_RentalModel(RentalModel rental)
    {
      var validationContext = new ValidationContext(rental);
      Assert.Empty(rental.Validate(validationContext));
    }
  }
}
