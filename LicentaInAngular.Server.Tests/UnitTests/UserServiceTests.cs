using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.UnitTests
{
    [TestFixture]
    public class UserServiceTests
    {
        private ApplicationDbContext _context = null!;
        private IConfiguration _configuration = null!;
        private Mock<ILogger<UserService>> _loggerMock = null!;
        private UserService _service = null!;

        [SetUp]
        public void Before()
        {
            _context = TestDbContextFactory.CreateContext();

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "TestSecretKeyForJwtTokenGeneration1234567890" },
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:ExpirationInMinutes", "60" }
                })
                .Build();

            _loggerMock = new Mock<ILogger<UserService>>();

            _service = new UserService(
                _context,
                _configuration,
                _loggerMock.Object
            );
        }

        [TearDown]
        public void After()
        {
            _context.Dispose();
        }

        private static object? GetAnonymousProperty(object source, string propertyName)
        {
            return source.GetType().GetProperty(propertyName)?.GetValue(source);
        }

        private static List<object> AnonymousList(object value)
        {
            return ((IEnumerable)value).Cast<object>().ToList();
        }

        private static Persoana CreatePersoana(
            int id = 1,
            string nume = "User",
            string prenume = "Test",
            string email = "user@test.com",
            string tipPersoana = "Fizica",
            string telefon = "0711111111",
            string rol = "Participant")
        {
            return new Persoana
            {
                IdPersoana = id,
                Nume = nume,
                Prenume = prenume,
                Email = email,
                tipPersoana = tipPersoana,
                Telefon = telefon,
                Rol = rol
            };
        }

        private static User CreateUser(
            int id = 1,
            string username = "user",
            string password = "Password123",
            int idPersoana = 1,
            bool hashPassword = true)
        {
            return new User
            {
                IdUser = id,
                Username = username,
                Password = hashPassword ? BCrypt.Net.BCrypt.HashPassword(password) : password,
                IdPersoana = idPersoana
            };
        }

        private static RegisterUserDto CreateRegisterDto(
            string username = "newuser",
            string password = "Password123",
            string email = "newuser@test.com",
            string role = "Participant")
        {
            return new RegisterUserDto
            {
                Username = username,
                Password = password,
                Persoana = new RegisterPersoanaDto
                {
                    Nume = "New",
                    Prenume = "User",
                    Email = email,
                    TipPersoana = "Fizica",
                    Telefon = "0711111111",
                    Rol = role
                }
            };
        }

        private static UpdateUserDto CreateUpdateUserDto(
            string username = "updateduser",
            string password = "",
            string email = "updated@test.com",
            string role = "Participant")
        {
            return new UpdateUserDto
            {
                Username = username,
                Password = password,
                Persoana = new UpdatePersoanaDto
                {
                    Nume = "Updated",
                    Prenume = "Person",
                    Email = email,
                    TipPersoana = "Fizica",
                    Telefon = "0722222222",
                    Rol = role
                }
            };
        }

        private async Task SeedUserWithPerson(
            int userId = 1,
            int personId = 1,
            string username = "user",
            string password = "Password123",
            string email = "user@test.com",
            string role = "Participant",
            string tipPersoana = "Fizica")
        {
            _context.Persoane.Add(CreatePersoana(
                personId,
                "User",
                "Test",
                email,
                tipPersoana,
                "0711111111",
                role
            ));

            _context.Users.Add(CreateUser(
                userId,
                username,
                password,
                personId,
                hashPassword: true
            ));

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task CreateUser_ShouldPersistUser()
        {
            await _service.CreateUser(new User
            {
                Username = "george",
                Password = "pass",
                IdPersoana = 1
            });

            ClassicAssert.AreEqual(1, _context.Users.Count(u => u.Username == "george"));
        }

        [Test]
        public async Task GetUserIdByUsername_WhenExists_ShouldReturnId()
        {
            _context.Users.Add(new User
            {
                IdUser = 10,
                Username = "admin",
                Password = "pass",
                IdPersoana = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetUserIdByUsername("admin");

            ClassicAssert.AreEqual(10, result);
        }

        [Test]
        public async Task GetUserIdByUsername_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetUserIdByUsername("missing");

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetByUsername_WhenExists_ShouldIncludePersoana()
        {
            await SeedUserWithPerson(1, 1, "admin", "Password123", "admin@test.com", "Administrator");

            var result = await _service.GetByUsername("admin");

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("admin", result!.Username);
            ClassicAssert.IsNotNull(result.Persoana);
            ClassicAssert.AreEqual("admin@test.com", result.Persoana!.Email);
            ClassicAssert.AreEqual("Administrator", result.Persoana.Rol);
        }

        [Test]
        public async Task GetByUsername_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetByUsername("missing");

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetByEmail_WhenExists_ShouldReturnMatchingUserIgnoringCase()
        {
            await SeedUserWithPerson(2, 2, "mailuser", "Password123", "mail@test.com");

            var result = await _service.GetByEmail("MAIL@test.com");

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("mailuser", result!.Username);
            ClassicAssert.IsNotNull(result.Persoana);
            ClassicAssert.AreEqual("mail@test.com", result.Persoana!.Email);
        }

        [Test]
        public async Task GetByEmail_WhenMissing_ShouldReturnNull()
        {
            await SeedUserWithPerson(1, 1, "user", "Password123", "user@test.com");

            var result = await _service.GetByEmail("missing@test.com");

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task GetById_WhenExists_ShouldReturnUserWithPersoana()
        {
            await SeedUserWithPerson(3, 3, "byid", "Password123", "byid@test.com");

            var result = await _service.GetById(3);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("byid", result!.Username);
            ClassicAssert.IsNotNull(result.Persoana);
            ClassicAssert.AreEqual("byid@test.com", result.Persoana!.Email);
        }

        [Test]
        public async Task GetById_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.GetById(999);

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task UpdateUser_WhenExists_ShouldUpdateUsernamePasswordAndPersoana()
        {
            await SeedUserWithPerson(3, 3, "olduser", "OldPassword123", "old@test.com");

            var result = await _service.UpdateUser(new User
            {
                IdUser = 3,
                Username = "newuser",
                Password = "NewPassword123",
                Persoana = new Persoana
                {
                    Nume = "New",
                    Prenume = "Name",
                    Email = "new@test.com",
                    tipPersoana = "Juridica",
                    Telefon = "0722222222"
                }
            });

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("newuser", result!.Username);
            ClassicAssert.AreNotEqual("NewPassword123", result.Password);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("NewPassword123", result.Password));
            ClassicAssert.IsNotNull(result.Persoana);
            ClassicAssert.AreEqual("New", result.Persoana!.Nume);
            ClassicAssert.AreEqual("Name", result.Persoana.Prenume);
            ClassicAssert.AreEqual("new@test.com", result.Persoana.Email);
            ClassicAssert.AreEqual("Juridica", result.Persoana.tipPersoana);
            ClassicAssert.AreEqual("0722222222", result.Persoana.Telefon);
        }

        [Test]
        public async Task UpdateUser_WhenSomeFieldsAreNull_ShouldKeepOldValues()
        {
            await SeedUserWithPerson(3, 3, "olduser", "OldPassword123", "old@test.com");

            var result = await _service.UpdateUser(new User
            {
                IdUser = 3,
                Username = null,
                Password = "",
                Persoana = new Persoana
                {
                    Nume = null,
                    Prenume = "ChangedPrenume",
                    Email = null,
                    tipPersoana = null,
                    Telefon = null
                }
            });

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("olduser", result!.Username);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("OldPassword123", result.Password));
            ClassicAssert.IsNotNull(result.Persoana);
            ClassicAssert.AreEqual("User", result.Persoana!.Nume);
            ClassicAssert.AreEqual("ChangedPrenume", result.Persoana.Prenume);
            ClassicAssert.AreEqual("old@test.com", result.Persoana.Email);
            ClassicAssert.AreEqual("Fizica", result.Persoana.tipPersoana);
        }

        [Test]
        public async Task UpdateUser_WhenPersoanaIsNull_ShouldOnlyUpdateUserFields()
        {
            await SeedUserWithPerson(3, 3, "olduser", "OldPassword123", "old@test.com");

            var result = await _service.UpdateUser(new User
            {
                IdUser = 3,
                Username = "onlyuser",
                Password = null,
                Persoana = null
            });

            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("onlyuser", result!.Username);
            ClassicAssert.IsNotNull(result.Persoana);
            ClassicAssert.AreEqual("old@test.com", result.Persoana!.Email);
        }

        [Test]
        public async Task UpdateUser_WhenMissing_ShouldReturnNull()
        {
            var result = await _service.UpdateUser(new User
            {
                IdUser = 999,
                Username = "none"
            });

            ClassicAssert.IsNull(result);
        }

        [Test]
        public async Task DeleteUser_WhenExists_ShouldReturnTrueAndRemoveUser()
        {
            _context.Users.Add(new User
            {
                IdUser = 50,
                Username = "delete",
                Password = "pass",
                IdPersoana = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.DeleteUser(50);

            var deleted = await _context.Users.FindAsync(50);

            ClassicAssert.IsTrue(result);
            ClassicAssert.IsNull(deleted);
        }

        [Test]
        public async Task DeleteUser_WhenMissing_ShouldReturnFalse()
        {
            var result = await _service.DeleteUser(12345);

            ClassicAssert.IsFalse(result);
        }

        [Test]
        public async Task GetUserIdByUsernameResponse_WhenUserExists_ShouldReturnOk()
        {
            _context.Users.Add(new User
            {
                IdUser = 10,
                Username = "admin",
                Password = "pass",
                IdPersoana = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.GetUserIdByUsernameResponse("admin");

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual(10, GetAnonymousProperty(ok.Value!, "IdUser"));
        }

        [Test]
        public async Task GetUserIdByUsernameResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetUserIdByUsernameResponse("missing");

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("User not found", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task GetAllUsersResponse_ShouldReturnOkWithUsers()
        {
            _context.Users.AddRange(
                new User { IdUser = 1, Username = "u1", Password = "p", IdPersoana = 1 },
                new User { IdUser = 2, Username = "u2", Password = "p", IdPersoana = 2 }
            );

            await _context.SaveChangesAsync();

            var result = await _service.GetAllUsersResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var users = AnonymousList(ok.Value!);

            ClassicAssert.AreEqual(2, users.Count);
            ClassicAssert.AreEqual(1, GetAnonymousProperty(users[0], "IdUser"));
            ClassicAssert.AreEqual("u1", GetAnonymousProperty(users[0], "Username"));
        }

        [Test]
        public async Task GetAllUsersResponse_WhenNoUsers_ShouldReturnOkWithEmptyList()
        {
            var result = await _service.GetAllUsersResponse();

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);

            var users = AnonymousList(ok.Value!);

            ClassicAssert.AreEqual(0, users.Count);
        }

        [Test]
        public async Task GetUserByUsernameResponse_WhenUserExists_ShouldReturnOk()
        {
            await SeedUserWithPerson(1, 1, "admin", "Password123", "admin@test.com");

            var result = await _service.GetUserByUsernameResponse("admin");

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsInstanceOf<User>(ok.Value);

            var user = ok.Value as User;

            ClassicAssert.IsNotNull(user);
            ClassicAssert.AreEqual("admin", user!.Username);
        }

        [Test]
        public async Task GetUserByUsernameResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.GetUserByUsernameResponse("missing");

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("User not found", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task RegisterResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.RegisterResponse(null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(400, badRequest!.StatusCode);
            ClassicAssert.AreEqual("Invalid request. Missing user or personal data.", GetAnonymousProperty(badRequest.Value!, "message"));
        }

        [Test]
        public async Task RegisterResponse_WhenPersoanaIsNull_ShouldReturnBadRequest()
        {
            var dto = CreateRegisterDto();
            dto.Persoana = null!;

            var result = await _service.RegisterResponse(dto);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Invalid request. Missing user or personal data.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task RegisterResponse_WhenEmailIsInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.RegisterResponse(CreateRegisterDto(email: "invalid-email"));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Invalid email format.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task RegisterResponse_WhenEmailAlreadyExists_ShouldReturnBadRequest()
        {
            await SeedUserWithPerson(1, 1, "existing", "Password123", "existing@test.com");

            var result = await _service.RegisterResponse(CreateRegisterDto(
                username: "newusername",
                email: "existing@test.com"
            ));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Email already exists.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task RegisterResponse_WhenUsernameAlreadyExists_ShouldReturnBadRequest()
        {
            await SeedUserWithPerson(1, 1, "existing", "Password123", "existing@test.com");

            var result = await _service.RegisterResponse(CreateRegisterDto(
                username: "existing",
                email: "other@test.com"
            ));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Username already exists.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task RegisterResponse_WhenPasswordIsInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.RegisterResponse(CreateRegisterDto(password: "weak"));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(
                "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number.",
                GetAnonymousProperty(badRequest!.Value!, "message")
            );
        }

        [Test]
        public async Task RegisterResponse_WhenRoleIsInvalid_ShouldReturnBadRequest()
        {
            var result = await _service.RegisterResponse(CreateRegisterDto(role: "InvalidRole"));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(
                "Invalid role. Allowed values: Owner, Administrator, Participant.",
                GetAnonymousProperty(badRequest!.Value!, "message")
            );
        }

        [Test]
        public async Task RegisterResponse_WhenValid_ShouldCreateAccountAndReturnOk()
        {
            var result = await _service.RegisterResponse(CreateRegisterDto(
                username: "newuser",
                password: "Password123",
                email: "newuser@test.com",
                role: "Participant"
            ));

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("Account created successfully.", GetAnonymousProperty(ok.Value!, "message"));

            ClassicAssert.AreEqual(1, _context.Persoane.Count(p => p.Email == "newuser@test.com"));
            ClassicAssert.AreEqual(1, _context.Users.Count(u => u.Username == "newuser"));

            var user = _context.Users.Single(u => u.Username == "newuser");

            ClassicAssert.AreNotEqual("Password123", user.Password);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("Password123", user.Password));
        }

        [Test]
        public async Task LoginResponse_WhenUsernameMissing_ShouldReturnUnauthorized()
        {
            var result = await _service.LoginResponse(new LoginDto
            {
                Username = "missing",
                Password = "Password123"
            });

            var unauthorized = result as UnauthorizedObjectResult;

            ClassicAssert.IsNotNull(unauthorized);
            ClassicAssert.AreEqual(401, unauthorized!.StatusCode);
            ClassicAssert.AreEqual("Invalid credentials", GetAnonymousProperty(unauthorized.Value!, "message"));
        }

        [Test]
        public async Task LoginResponse_WhenPasswordIsWrong_ShouldReturnUnauthorized()
        {
            await SeedUserWithPerson(1, 1, "loginuser", "Password123", "login@test.com");

            var result = await _service.LoginResponse(new LoginDto
            {
                Username = "loginuser",
                Password = "WrongPassword123"
            });

            var unauthorized = result as UnauthorizedObjectResult;

            ClassicAssert.IsNotNull(unauthorized);
            ClassicAssert.AreEqual("Invalid credentials", GetAnonymousProperty(unauthorized!.Value!, "message"));
        }

        [Test]
        public async Task LoginResponse_WhenValid_ShouldReturnTokenAndUserData()
        {
            await SeedUserWithPerson(1, 1, "loginuser", "Password123", "login@test.com", "Administrator");

            var result = await _service.LoginResponse(new LoginDto
            {
                Username = "loginuser",
                Password = "Password123"
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.IsNotNull(GetAnonymousProperty(ok.Value!, "token"));

            var userObject = GetAnonymousProperty(ok.Value!, "user");

            ClassicAssert.IsNotNull(userObject);
            ClassicAssert.AreEqual("loginuser", GetAnonymousProperty(userObject!, "Username"));
        }

        [Test]
        public async Task LoginResponse_WhenCredentialsIsNull_ShouldReturnInternalServerError()
        {
            var result = await _service.LoginResponse(null!);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("An error occurred while processing your request.", GetAnonymousProperty(objectResult.Value!, "message"));
        }

        [Test]
        public async Task UpdateUserResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.UpdateUserResponse(1, null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Invalid request. Missing user or personal data.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task UpdateUserResponse_WhenPersoanaIsNull_ShouldReturnBadRequest()
        {
            var dto = CreateUpdateUserDto();
            dto.Persoana = null!;

            var result = await _service.UpdateUserResponse(1, dto);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Invalid request. Missing user or personal data.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task UpdateUserResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.UpdateUserResponse(999, CreateUpdateUserDto());

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("User not found.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task UpdateUserResponse_WhenEmailInvalid_ShouldReturnBadRequest()
        {
            await SeedUserWithPerson(1, 1, "user", "Password123", "user@test.com");

            var result = await _service.UpdateUserResponse(1, CreateUpdateUserDto(email: "bad-email"));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Invalid email format.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task UpdateUserResponse_WhenEmailAlreadyExistsForAnotherUser_ShouldReturnBadRequest()
        {
            await SeedUserWithPerson(1, 1, "user1", "Password123", "user1@test.com");
            await SeedUserWithPerson(2, 2, "user2", "Password123", "user2@test.com");

            var result = await _service.UpdateUserResponse(1, CreateUpdateUserDto(
                username: "user1updated",
                email: "user2@test.com"
            ));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Email already exists.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task UpdateUserResponse_WhenUsernameAlreadyExistsForAnotherUser_ShouldReturnBadRequest()
        {
            await SeedUserWithPerson(1, 1, "user1", "Password123", "user1@test.com");
            await SeedUserWithPerson(2, 2, "user2", "Password123", "user2@test.com");

            var result = await _service.UpdateUserResponse(1, CreateUpdateUserDto(
                username: "user2",
                email: "newemail@test.com"
            ));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Username already exists.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task UpdateUserResponse_WhenRoleInvalid_ShouldReturnBadRequest()
        {
            await SeedUserWithPerson(1, 1, "user", "Password123", "user@test.com");

            var result = await _service.UpdateUserResponse(1, CreateUpdateUserDto(role: "InvalidRole"));

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual(
                "Invalid role. Allowed values: Owner, Administrator, Participant.",
                GetAnonymousProperty(badRequest!.Value!, "message")
            );
        }

        [Test]
        public async Task UpdateUserResponse_WhenAssociatedPersoanaMissing_ShouldReturnNotFound()
        {
            _context.Users.Add(new User
            {
                IdUser = 1,
                Username = "orphan",
                Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
                IdPersoana = 999
            });

            await _context.SaveChangesAsync();

            var result = await _service.UpdateUserResponse(1, CreateUpdateUserDto());

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Associated personal data not found.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task UpdateUserResponse_WhenValidWithoutPassword_ShouldUpdateUserAndPersoana()
        {
            await SeedUserWithPerson(1, 1, "user", "OldPassword123", "user@test.com");

            var result = await _service.UpdateUserResponse(1, CreateUpdateUserDto(
                username: "updateduser",
                password: "",
                email: "updated@test.com",
                role: "Administrator"
            ));

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("User updated successfully.", GetAnonymousProperty(ok.Value!, "message"));

            var updated = await _service.GetById(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.AreEqual("updateduser", updated!.Username);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("OldPassword123", updated.Password));
            ClassicAssert.IsNotNull(updated.Persoana);
            ClassicAssert.AreEqual("updated@test.com", updated.Persoana!.Email);
            ClassicAssert.AreEqual("Administrator", updated.Persoana.Rol);
        }

        [Test]
        public async Task UpdateUserResponse_WhenValidWithPassword_ShouldHashAndUpdatePassword()
        {
            await SeedUserWithPerson(1, 1, "user", "OldPassword123", "user@test.com");

            var result = await _service.UpdateUserResponse(1, CreateUpdateUserDto(
                username: "updateduser",
                password: "NewPassword123",
                email: "updated@test.com",
                role: "Participant"
            ));

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var updated = await _service.GetById(1);

            ClassicAssert.IsNotNull(updated);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("NewPassword123", updated!.Password));
        }

        [Test]
        public async Task DeleteUserResponse_WhenUserExists_ShouldReturnOk()
        {
            _context.Users.Add(new User
            {
                IdUser = 1,
                Username = "delete",
                Password = "pass",
                IdPersoana = 1
            });

            await _context.SaveChangesAsync();

            var result = await _service.DeleteUserResponse(1);

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual(200, ok!.StatusCode);
            ClassicAssert.AreEqual("User deleted successfully", GetAnonymousProperty(ok.Value!, "message"));
        }

        [Test]
        public async Task DeleteUserResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.DeleteUserResponse(999);

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual(404, notFound!.StatusCode);
            ClassicAssert.AreEqual("User not found or already deleted", GetAnonymousProperty(notFound.Value!, "message"));
        }

        [Test]
        public async Task UpdatePasswordResponse_WhenPasswordTooShort_ShouldReturnBadRequest()
        {
            var result = await _service.UpdatePasswordResponse(1, "123");

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Password must be at least 6 characters long.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task UpdatePasswordResponse_WhenPasswordIsWhiteSpace_ShouldReturnBadRequest()
        {
            var result = await _service.UpdatePasswordResponse(1, "   ");

            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdatePasswordResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.UpdatePasswordResponse(999, "NewPass123");

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("User not found.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task UpdatePasswordResponse_WhenValid_ShouldHashAndUpdatePassword()
        {
            await SeedUserWithPerson(1, 1, "user", "OldPass123", "user@test.com");

            var result = await _service.UpdatePasswordResponse(1, "NewPass123");

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Password updated successfully.", GetAnonymousProperty(ok!.Value!, "message"));

            var user = await _context.Users.FindAsync(1);

            ClassicAssert.IsNotNull(user);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("NewPass123", user!.Password));
        }

        [Test]
        public async Task ChangePasswordWithOldResponse_WhenDtoIsNull_ShouldReturnBadRequest()
        {
            var result = await _service.ChangePasswordWithOldResponse(1, null!);

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Old and new passwords are required.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task ChangePasswordWithOldResponse_WhenOldPasswordMissing_ShouldReturnBadRequest()
        {
            var result = await _service.ChangePasswordWithOldResponse(1, new AccountController.ChangePasswordDto
            {
                OldPassword = "",
                NewPassword = "NewPass123"
            });

            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ChangePasswordWithOldResponse_WhenNewPasswordTooShort_ShouldReturnBadRequest()
        {
            var result = await _service.ChangePasswordWithOldResponse(1, new AccountController.ChangePasswordDto
            {
                OldPassword = "OldPass123",
                NewPassword = "123"
            });

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("New password must be at least 6 characters long.", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task ChangePasswordWithOldResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.ChangePasswordWithOldResponse(999, new AccountController.ChangePasswordDto
            {
                OldPassword = "OldPass123",
                NewPassword = "NewPass123"
            });

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("User not found.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task ChangePasswordWithOldResponse_WhenOldPasswordIncorrect_ShouldReturnUnauthorized()
        {
            await SeedUserWithPerson(1, 1, "user", "OldPass123", "user@test.com");

            var result = await _service.ChangePasswordWithOldResponse(1, new AccountController.ChangePasswordDto
            {
                OldPassword = "WrongPass123",
                NewPassword = "NewPass123"
            });

            var unauthorized = result as UnauthorizedObjectResult;

            ClassicAssert.IsNotNull(unauthorized);
            ClassicAssert.AreEqual(401, unauthorized!.StatusCode);
            ClassicAssert.AreEqual("Old password is incorrect.", GetAnonymousProperty(unauthorized.Value!, "message"));
        }

        [Test]
        public async Task ChangePasswordWithOldResponse_WhenValid_ShouldUpdatePassword()
        {
            await SeedUserWithPerson(1, 1, "user", "OldPass123", "user@test.com");

            var result = await _service.ChangePasswordWithOldResponse(1, new AccountController.ChangePasswordDto
            {
                OldPassword = "OldPass123",
                NewPassword = "NewPass123"
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Password changed successfully.", GetAnonymousProperty(ok!.Value!, "message"));

            var user = await _context.Users.FindAsync(1);

            ClassicAssert.IsNotNull(user);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("NewPass123", user!.Password));
        }

        [Test]
        public async Task UpdateDetaliiUserResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.UpdateDetaliiUserResponse(999, new AccountController.UpdateDetaliiUserDTO
            {
                Username = "new"
            });

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Utilizatorul nu exista.", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task UpdateDetaliiUserResponse_WhenValid_ShouldUpdateOnlyProvidedFields()
        {
            await SeedUserWithPerson(1, 1, "olduser", "OldPass123", "old@test.com");

            var result = await _service.UpdateDetaliiUserResponse(1, new AccountController.UpdateDetaliiUserDTO
            {
                Username = "newuser",
                Password = "NewPass123",
                Nume = "NewName",
                Prenume = "NewPrenume",
                Email = "new@test.com",
                Telefon = "0799999999"
            });

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Detaliile au fost actualizate cu succes!", GetAnonymousProperty(ok!.Value!, "message"));

            var user = await _service.GetById(1);

            ClassicAssert.IsNotNull(user);
            ClassicAssert.AreEqual("newuser", user!.Username);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("NewPass123", user.Password));
            ClassicAssert.IsNotNull(user.Persoana);
            ClassicAssert.AreEqual("NewName", user.Persoana!.Nume);
            ClassicAssert.AreEqual("NewPrenume", user.Persoana.Prenume);
            ClassicAssert.AreEqual("new@test.com", user.Persoana.Email);
            ClassicAssert.AreEqual("0799999999", user.Persoana.Telefon);
        }

        [Test]
        public async Task UpdateDetaliiUserResponse_WhenFieldsAreBlank_ShouldKeepExistingValues()
        {
            await SeedUserWithPerson(1, 1, "olduser", "OldPass123", "old@test.com");

            var result = await _service.UpdateDetaliiUserResponse(1, new AccountController.UpdateDetaliiUserDTO
            {
                Username = "   ",
                Password = "",
                Nume = null,
                Prenume = "   ",
                Email = "",
                Telefon = null
            });

            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var user = await _service.GetById(1);

            ClassicAssert.IsNotNull(user);
            ClassicAssert.AreEqual("olduser", user!.Username);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify("OldPass123", user.Password));
            ClassicAssert.IsNotNull(user.Persoana);
            ClassicAssert.AreEqual("User", user.Persoana!.Nume);
            ClassicAssert.AreEqual("Test", user.Persoana.Prenume);
            ClassicAssert.AreEqual("old@test.com", user.Persoana.Email);
        }

        [Test]
        public async Task UpdateDetaliiUserResponse_WhenDtoIsNullAndUserExists_ShouldReturnInternalServerError()
        {
            await SeedUserWithPerson(1, 1, "user", "Password123", "user@test.com");

            var result = await _service.UpdateDetaliiUserResponse(1, null!);

            var objectResult = result as ObjectResult;

            ClassicAssert.IsNotNull(objectResult);
            ClassicAssert.AreEqual(500, objectResult!.StatusCode);
            ClassicAssert.AreEqual("An error occurred while updating the user details.", GetAnonymousProperty(objectResult.Value!, "message"));
        }

        [Test]
        public async Task UpdateRolResponse_WhenUserMissing_ShouldReturnNotFound()
        {
            var result = await _service.UpdateRolResponse(999, "Administrator");

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Utilizatorul nu a fost gasit!", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task UpdateRolResponse_WhenPersoanaMissing_ShouldReturnNotFound()
        {
            _context.Users.Add(new User
            {
                IdUser = 1,
                Username = "orphan",
                Password = "pass",
                IdPersoana = 999
            });

            await _context.SaveChangesAsync();

            var result = await _service.UpdateRolResponse(1, "Administrator");

            var notFound = result as NotFoundObjectResult;

            ClassicAssert.IsNotNull(notFound);
            ClassicAssert.AreEqual("Utilizatorul nu a fost gasit!", GetAnonymousProperty(notFound!.Value!, "message"));
        }

        [Test]
        public async Task UpdateRolResponse_WhenRoleInvalid_ShouldReturnBadRequest()
        {
            await SeedUserWithPerson(1, 1, "user", "Password123", "user@test.com");

            var result = await _service.UpdateRolResponse(1, "InvalidRole");

            var badRequest = result as BadRequestObjectResult;

            ClassicAssert.IsNotNull(badRequest);
            ClassicAssert.AreEqual("Rol invalid!", GetAnonymousProperty(badRequest!.Value!, "message"));
        }

        [Test]
        public async Task UpdateRolResponse_WhenValid_ShouldUpdateRole()
        {
            await SeedUserWithPerson(1, 1, "user", "Password123", "user@test.com", "Participant");

            var result = await _service.UpdateRolResponse(1, "Administrator");

            var ok = result as OkObjectResult;

            ClassicAssert.IsNotNull(ok);
            ClassicAssert.AreEqual("Rolul a fost actualizat la: Administrator", GetAnonymousProperty(ok!.Value!, "message"));

            var user = await _service.GetById(1);

            ClassicAssert.IsNotNull(user);
            ClassicAssert.IsNotNull(user!.Persoana);
            ClassicAssert.AreEqual("Administrator", user.Persoana!.Rol);
        }
    }
}
