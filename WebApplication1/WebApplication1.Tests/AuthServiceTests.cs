using NUnit.Framework;
using System;

namespace WebApplication1.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private AuthService authService;

        [SetUp]
        public void Setup()
        {
            // Створюємо унікальне ім'я файлу для кожного тесту
            string tempFile = $"test_users_{Guid.NewGuid()}.json";
            authService = new AuthService(tempFile);
        }

        [Test]
        public void Register_ValidUser_ShouldPass()
        {
            var user = new User
            {
                Login = "testuser123",
                Password = "12345"
            };

            authService.Register(user);

            Assert.That(user.Id, Is.GreaterThan(0));
        }

        [Test]
        public void Register_EmptyLogin_ShouldThrowException()
        {
            var user = new User
            {
                Login = "",
                Password = "12345"
            };

            // Обгорнули в new Action
            Assert.Throws<ArgumentException>(new Action(() => authService.Register(user)));
        }

        [Test]
        public void Authenticate_ValidCredentials_ShouldReturnUser()
        {
            var user = new User
            {
                Login = "authuser",
                Password = "pass123"
            };

            authService.Register(user);

            var result = authService.Authenticate("authuser", "pass123");

            Assert.That(result, Is.Not.Null);

            // Якщо Token підкреслює червоним (бо його немає в моделі User) - просто видали цей рядок:
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void Authenticate_InvalidPassword_ShouldReturnNull()
        {
            var result = authService.Authenticate("wrong", "wrong");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void UpdateUserProfile_InvalidPhone_ShouldReturnFalse()
        {
            var user = new User
            {
                Login = "phoneuser",
                Password = "12345"
            };

            authService.Register(user);

            var authUser = authService.Authenticate("phoneuser", "12345");

            // Якщо Phone підкреслює червоним - закоментуй:
            authUser.Phone = "INVALID_PHONE";

            var result = authService.UpdateUserProfile(authUser);

            Assert.That(result, Is.False);
        }
    }
}