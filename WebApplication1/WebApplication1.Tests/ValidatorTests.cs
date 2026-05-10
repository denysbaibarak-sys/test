using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace WebApplication1.Tests
{
    [TestFixture]
    public class ValidatorTests
    {
        private Validator validator;

        [SetUp]
        public void Setup()
        {
            validator = new Validator();
        }

        [Test]
        public void ValidateRestaurant_NullRestaurant_ShouldThrow()
        {
            // Обгорнули в new Action, щоб зняти двозначність
            Assert.Throws<ArgumentException>(new Action(() => validator.ValidateRestaurant(null)));
        }

        [Test]
        public void ValidateRestaurant_EmptyName_ShouldThrow()
        {
            var restaurant = new Restaurant
            {
                Name = "",
                Menu = new List<Food>()
            };

            Assert.Throws<ArgumentException>(new Action(() => validator.ValidateRestaurant(restaurant)));
        }

        [Test]
        public void ValidateRestaurant_NullMenu_ShouldThrow()
        {
            var restaurant = new Restaurant
            {
                Name = "Pizza Place",
                Menu = null
            };

            Assert.Throws<ArgumentException>(new Action(() => validator.ValidateRestaurant(restaurant)));
        }

        [Test]
        public void ValidateRestaurant_ValidRestaurant_ShouldPass()
        {
            var restaurant = new Restaurant
            {
                Name = "Pizza Place",
                Menu = new List<Food>()
            };

            validator.ValidateRestaurant(restaurant);
        }
    }
}