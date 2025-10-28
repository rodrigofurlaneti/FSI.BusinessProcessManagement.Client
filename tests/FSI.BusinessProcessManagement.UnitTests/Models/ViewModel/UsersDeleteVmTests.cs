using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class UsersDeleteVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new UsersDeleteVm();

            vm.UserId.Should().Be(0);
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new UsersDeleteVm
            {
                UserId = 42
            };

            vm.UserId.Should().Be(42);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new UsersDeleteVm { UserId = 99 };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<UsersDeleteVm>(json, options);

            copy.Should().NotBeNull();
            copy!.UserId.Should().Be(original.UserId);
        }
    }
}
