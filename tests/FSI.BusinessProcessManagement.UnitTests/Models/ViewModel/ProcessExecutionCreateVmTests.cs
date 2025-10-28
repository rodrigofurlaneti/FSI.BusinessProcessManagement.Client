using System;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class ProcessExecutionCreateVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new ProcessExecutionCreateVm();

            vm.ProcessId.Should().Be(0);
            vm.StepId.Should().Be(0);
            vm.UserId.Should().BeNull();

            vm.Status.Should().NotBeNull();
            vm.Status.Should().Be(string.Empty);

            vm.StartedAt.Should().BeNull();
            vm.CompletedAt.Should().BeNull();
            vm.Remarks.Should().BeNull();
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var now = DateTime.UtcNow;

            var vm = new ProcessExecutionCreateVm
            {
                ProcessId = 10,
                StepId = 20,
                UserId = 30,
                Status = "Running",
                StartedAt = now.AddMinutes(-15),
                CompletedAt = now,
                Remarks = "Execução em andamento"
            };

            vm.ProcessId.Should().Be(10);
            vm.StepId.Should().Be(20);
            vm.UserId.Should().Be(30);
            vm.Status.Should().Be("Running");
            vm.StartedAt.Should().BeCloseTo(now.AddMinutes(-15), TimeSpan.FromSeconds(1));
            vm.CompletedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            vm.Remarks.Should().Be("Execução em andamento");
        }

        [Fact]
        public void Nullable_Properties_Should_Accept_Null()
        {
            var vm = new ProcessExecutionCreateVm
            {
                UserId = null,
                StartedAt = null,
                CompletedAt = null,
                Remarks = null
            };

            vm.UserId.Should().BeNull();
            vm.StartedAt.Should().BeNull();
            vm.CompletedAt.Should().BeNull();
            vm.Remarks.Should().BeNull();
        }

        [Fact]
        public void Allows_Any_Timestamps_Without_Implicit_Validation()
        {
            // CompletedAt antes de StartedAt (não há validação no VM)
            var start = DateTime.UtcNow;
            var end = start.AddMinutes(-5);

            var vm = new ProcessExecutionCreateVm
            {
                StartedAt = start,
                CompletedAt = end
            };

            vm.StartedAt.Should().Be(start);
            vm.CompletedAt.Should().Be(end);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new ProcessExecutionCreateVm
            {
                ProcessId = 1,
                StepId = 2,
                UserId = 3,
                Status = "Completed",
                StartedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-1), DateTimeKind.Utc),
                CompletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                Remarks = "OK"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<ProcessExecutionCreateVm>(json, options);

            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }
    }
}
