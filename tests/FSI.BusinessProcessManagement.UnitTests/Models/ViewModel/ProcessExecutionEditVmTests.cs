using System;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class ProcessExecutionEditVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new ProcessExecutionEditVm();

            vm.ExecutionId.Should().Be(0);
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

            var vm = new ProcessExecutionEditVm
            {
                ExecutionId = 1,
                ProcessId = 2,
                StepId = 3,
                UserId = 4,
                Status = "Running",
                StartedAt = now.AddMinutes(-5),
                CompletedAt = now,
                Remarks = "Tudo certo"
            };

            vm.ExecutionId.Should().Be(1);
            vm.ProcessId.Should().Be(2);
            vm.StepId.Should().Be(3);
            vm.UserId.Should().Be(4);
            vm.Status.Should().Be("Running");
            vm.StartedAt.Should().BeCloseTo(now.AddMinutes(-5), TimeSpan.FromSeconds(1));
            vm.CompletedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            vm.Remarks.Should().Be("Tudo certo");
        }

        [Fact]
        public void Nullable_Properties_Should_Accept_Null()
        {
            var vm = new ProcessExecutionEditVm
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
            // CompletedAt anterior a StartedAt — permitido no ViewModel
            var start = DateTime.UtcNow;
            var end = start.AddMinutes(-10);

            var vm = new ProcessExecutionEditVm
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
            var original = new ProcessExecutionEditVm
            {
                ExecutionId = 100,
                ProcessId = 200,
                StepId = 300,
                UserId = 400,
                Status = "Completed",
                StartedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-2), DateTimeKind.Utc),
                CompletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                Remarks = "Processo finalizado com sucesso"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<ProcessExecutionEditVm>(json, options);

            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }
    }
}
