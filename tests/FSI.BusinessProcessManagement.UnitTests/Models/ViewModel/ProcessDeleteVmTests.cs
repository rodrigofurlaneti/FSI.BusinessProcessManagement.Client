using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class ProcessDeleteVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new ProcessDeleteVm();

            vm.ProcessId.Should().Be(0);
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new ProcessDeleteVm
            {
                ProcessId = 99
            };

            vm.ProcessId.Should().Be(99);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new ProcessDeleteVm { ProcessId = 7 };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<ProcessDeleteVm>(json, options);

            copy.Should().NotBeNull();
            copy!.ProcessId.Should().Be(original.ProcessId);
        }
    }
}
