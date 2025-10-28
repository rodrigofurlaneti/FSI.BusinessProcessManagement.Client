using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class ProcessExecutionDeleteVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new ProcessExecutionDeleteVm();

            vm.ExecutionId.Should().Be(0);
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new ProcessExecutionDeleteVm
            {
                ExecutionId = 77
            };

            vm.ExecutionId.Should().Be(77);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new ProcessExecutionDeleteVm { ExecutionId = 10 };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<ProcessExecutionDeleteVm>(json, options);

            copy.Should().NotBeNull();
            copy!.ExecutionId.Should().Be(original.ExecutionId);
        }
    }
}
