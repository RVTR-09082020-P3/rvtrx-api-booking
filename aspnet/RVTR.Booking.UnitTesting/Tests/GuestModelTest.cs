using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RVTR.Booking.ObjectModel.Models;
using Xunit;

namespace RVTR.Booking.UnitTesting.Tests
{
  public class GuestModelTest
  {
    public static readonly IEnumerable<Object[]> _guests = new List<Object[]>
    {
      new object[]
      {
        new GuestModel()
        {
          Id = 0,
          BookingId = 0,
          Booking = null
        }
      }
    };

    [Theory]
    [MemberData(nameof(_guests))]
    public void Test_Create_GuestModel(GuestModel guest)
    {
      var validationContext = new ValidationContext(guest);
      var actual = Validator.TryValidateObject(guest, validationContext, null, true);

      Assert.True(actual);
    }

    [Theory]
    [MemberData(nameof(_guests))]
    public void Test_Validate_GuestModel(GuestModel guest)
    {
      var validationContext = new ValidationContext(guest);
      Assert.Empty(guest.Validate(validationContext));
    }
  }
}
