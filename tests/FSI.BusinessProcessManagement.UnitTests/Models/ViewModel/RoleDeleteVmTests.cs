using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class RoleDeleteVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new RoleDeleteVm();

            vm.RoleId.Should().Be(0);
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new RoleDeleteVm
            {
                RoleId = 42
            };

            vm.RoleId.Should().Be(42);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new RoleDeleteVm { RoleId = 100 };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<RoleDeleteVm>(json, options);

            copy.Should().NotBeNull();
            copy!.RoleId.Should().Be(original.RoleId);
        }
    }
}
