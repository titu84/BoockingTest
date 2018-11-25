using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Booking.Tests
{
    [TestFixture]
    public class BookingTestClass
    {
        private Booking _existingBooking;
        private Mock<IBookingRepository> _repository;

        [SetUp]
        public void Init()
        {
            var date = new DateTime(2018, 1, 1);
            _existingBooking = new Booking()
            {
                Id = 1,
                ArrivalDate = date,
                DepartureDate = After(date, 10),
                Reference = "X"
            };
            _repository = new Mock<IBookingRepository>();
            _repository.Setup(r => r.GetActiveBookings(It.IsAny<int>())).Returns(new List<Booking>
            {
              _existingBooking
            }.AsQueryable());            
        }

        //Sprawdzenie istniejącej rezewacji
        [Test]
        public void BookingIsExistingBooking_ReturnReference()
        {
            var result = BookingHelper.OverlappingBookingsExist(new Booking
            {
                Id = 1
            }, _repository.Object);
            Assert.AreEqual(result, _existingBooking.Reference);
        }

        //Rezerwacja jest anulowana
        [Test]
        public void BookingIsBookingCancelled_ReturnEmptyString()
        {
            var result = BookingHelper.OverlappingBookingsExist(new Booking
            {                            
                Status = "Cancelled"
            }, _repository.Object);
            Assert.That(result, Is.Empty);
        }

        //Rezerwacja zaczyna się i kończy się przed istniejącą rezerwacją
        [Test]
        public void BookingStartsAndFinishesBeforeAnExistingBooking_ReturnEmptyString()
        {
            var result = BookingHelper.OverlappingBookingsExist(new Booking
            {
                Id = 2,
                ArrivalDate = Before(_existingBooking.ArrivalDate, days: 2),
                DepartureDate = Before(_existingBooking.ArrivalDate)
            }, _repository.Object);
            Assert.That(result, Is.Empty);
        }

        //Rezerwacja zaczyna się i kończy się po istniejącej rezerwacji
        [Test]
        public void BookingStartsAndFinishesAfterAnExistingBooking_ReturnEmptyString()
        {
            var result = BookingHelper.OverlappingBookingsExist(new Booking
            {
                Id = 2,
                ArrivalDate = After(_existingBooking.DepartureDate),
                DepartureDate = After(_existingBooking.DepartureDate, days: 2)
            }, _repository.Object);
            Assert.That(result, Is.Empty);
        }

        //Rezerwacja konczy się w trakcie innej rezerwacji
        [Test]
        public void BookingFinishesInExistingBooking_ReturnReference()
        {            
            var result = BookingHelper.OverlappingBookingsExist(new Booking
            {
                Id = 2,
                DepartureDate = After(_existingBooking.ArrivalDate)
            }, _repository.Object);
            Assert.AreEqual(result, _existingBooking.Reference);
        }
        //Rezerwacja zaczyna się w trakcie innej rezerwacji
        [Test]
        public void BookingStartInExistingBooking_ReturnReference()
        {            
            var result = BookingHelper.OverlappingBookingsExist(new Booking
            {
                Id = 2,
                ArrivalDate = After(_existingBooking.ArrivalDate)              
            }, _repository.Object);
            Assert.AreEqual(result, _existingBooking.Reference);
        }

        //Rezerwacja zaczyna się przed rozpoczęciem istniejącej rezerwacji i kończy po jej zakończeniu
        [Test]
        public void BookingStartsBeforeAndFinishesAfterAnExistingBooking_ReturnEmptyString()
        {
            var result = BookingHelper.OverlappingBookingsExist(new Booking
            {
                Id = 2,
                ArrivalDate = Before(_existingBooking.ArrivalDate),
                DepartureDate = After(_existingBooking.DepartureDate, days: 2)
            }, _repository.Object);
            Assert.AreEqual(result, _existingBooking.Reference);
        }
        private DateTime Before(DateTime dateTime, int days = 1)
        {
            return dateTime.AddDays(-days);
        }
        private DateTime After(DateTime dateTime, int days = 1)
        {
            return dateTime.AddDays(days);
        }
        private DateTime ArriveOn(int year, int month, int day)
        {
            return new DateTime(year, month, day, 14, 0, 0);
        }
        private DateTime DepartOn(int year, int month, int day)
        {
            return new DateTime(year, month, day, 10, 0, 0);
        }
    }
}
