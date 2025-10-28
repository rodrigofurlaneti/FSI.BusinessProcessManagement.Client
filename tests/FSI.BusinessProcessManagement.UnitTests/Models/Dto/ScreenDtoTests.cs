using FluentAssertions;
using FSI.BusinessProcessManagement.Models.Dto;
using FSI.BusinessProcessManagement.Models.ViewModel;
using System.Text.Json;
using Xunit;

namespace FSI.BusinessProcessManagement.Models.Dto.Tests
{
    public sealed class ScreenDtoTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            // Arrange & Act
            var dto = new ScreenDto();

            // Assert
            dto.Id.Should().Be(0);
            dto.Name.Should().NotBeNull();
            dto.Name.Should().Be(string.Empty);
            dto.Description.Should().BeNull();
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            // Arrange
            var dto = new ScreenDto
            {
                Id = 10,
                Name = "Dashboard",
                Description = "Tela principal do sistema"
            };

            // Assert
            dto.Id.Should().Be(10);
            dto.Name.Should().Be("Dashboard");
            dto.Description.Should().Be("Tela principal do sistema");
        }

        [Fact]
        public void Nullable_Properties_Should_Accept_Null()
        {
            // Arrange
            var dto = new ScreenDto
            {
                Description = null
            };

            // Assert
            dto.Description.Should().BeNull();
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            // Arrange
            var original = new ScreenDto
            {
                Id = 1,
                Name = "Configurações",
                Description = "Tela de configuração geral"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            // Act
            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<ScreenDto>(json, options);

            // Assert
            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }
    }
}
