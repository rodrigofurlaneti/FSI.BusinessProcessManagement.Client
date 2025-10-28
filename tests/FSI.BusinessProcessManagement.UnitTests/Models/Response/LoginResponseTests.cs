using System;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.Response;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.Response
{
    public sealed class LoginResponseTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var dto = new LoginResponse();

            dto.AccessToken.Should().Be(string.Empty);
            dto.TokenType.Should().Be("Bearer");
            dto.ExpiresAtUtc.Should().Be(default); // 0001-01-01
            dto.UserId.Should().Be(0);
            dto.Username.Should().Be(string.Empty);
            dto.Roles.Should().NotBeNull();
            dto.Roles.Should().BeEmpty();
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var expires = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Utc);

            var dto = new LoginResponse
            {
                AccessToken = "token-abc",
                TokenType = "Bearer",
                ExpiresAtUtc = expires,
                UserId = 42,
                Username = "rodrigo",
                Roles = new[] { "Administrador", "Operador" }
            };

            dto.AccessToken.Should().Be("token-abc");
            dto.TokenType.Should().Be("Bearer");
            dto.ExpiresAtUtc.Should().BeCloseTo(expires, TimeSpan.FromSeconds(1));
            dto.UserId.Should().Be(42);
            dto.Username.Should().Be("rodrigo");
            dto.Roles.Should().BeEquivalentTo(new[] { "Administrador", "Operador" });
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new LoginResponse
            {
                AccessToken = "jwt-123",
                TokenType = "Bearer",
                ExpiresAtUtc = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(30), DateTimeKind.Utc),
                UserId = 7,
                Username = "admin",
                Roles = new[] { "Admin", "User" }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<LoginResponse>(json, options);

            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }

        [Fact]
        public void Roles_Default_Should_Be_Empty_Not_Null()
        {
            var dto = new LoginResponse();
            dto.Roles.Should().NotBeNull().And.BeEmpty();
        }
    }
}
