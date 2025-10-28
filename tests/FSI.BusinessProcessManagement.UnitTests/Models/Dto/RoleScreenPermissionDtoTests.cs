using FluentAssertions;
using FSI.BusinessProcessManagement.Models.Dto;
using FSI.BusinessProcessManagement.Models.Request;
using System.Text.Json;
using Xunit;

namespace FSI.BusinessProcessManagement.Models.Dto.Tests
{
    public sealed class RoleScreenPermissionDtoTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var dto = new RoleScreenPermissionDto();

            dto.Id.Should().Be(0);
            dto.RoleId.Should().Be(0);
            dto.ScreenId.Should().Be(0);

            // bool → default = false
            dto.CanView.Should().BeFalse();
            dto.CanCreate.Should().BeFalse();
            dto.CanEdit.Should().BeFalse();
            dto.CanDelete.Should().BeFalse();
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var dto = new RoleScreenPermissionDto
            {
                Id = 10,
                RoleId = 20,
                ScreenId = 30,
                CanView = true,
                CanCreate = true,
                CanEdit = false,
                CanDelete = true
            };

            dto.Id.Should().Be(10);
            dto.RoleId.Should().Be(20);
            dto.ScreenId.Should().Be(30);
            dto.CanView.Should().BeTrue();
            dto.CanCreate.Should().BeTrue();
            dto.CanEdit.Should().BeFalse();
            dto.CanDelete.Should().BeTrue();
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new RoleScreenPermissionDto
            {
                Id = 1,
                RoleId = 2,
                ScreenId = 3,
                CanView = true,
                CanCreate = false,
                CanEdit = true,
                CanDelete = false
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<RoleScreenPermissionDto>(json, options);

            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }
    }

}

