using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using VerifyXunit;
using VerifyTests;
using FSI.BusinessProcessManagement.Models.Request;

namespace FSI.BusinessProcessManagement.UnitTests.Models.Request
{
    public sealed class LoginRequestTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var dto = new LoginRequest();

            dto.Username.Should().NotBeNull();
            dto.Username.Should().Be(string.Empty);

            dto.Password.Should().NotBeNull();
            dto.Password.Should().Be(string.Empty);
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var dto = new LoginRequest
            {
                Username = "rodrigo",
                Password = "12345"
            };

            dto.Username.Should().Be("rodrigo");
            dto.Password.Should().Be("12345");
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new LoginRequest
            {
                Username = "admin",
                Password = "secret"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<LoginRequest>(json, options);

            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }

        [Fact]
        public void Should_Accept_Null_Assignment_If_Forced()
        {
            var dto = new LoginRequest
            {
                Username = null!,
                Password = null!
            };

            dto.Username.Should().BeNull();
            dto.Password.Should().BeNull();
        }
    }
}
