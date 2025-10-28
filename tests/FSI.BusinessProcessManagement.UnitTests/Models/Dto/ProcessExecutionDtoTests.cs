using System;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.Dto;
using Xunit;

namespace FSI.BusinessProcessManagement.Models.Dto.Tests
{
    public sealed class ProcessExecutionDtoTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            // Act
            var dto = new ProcessExecutionDto();

            // Assert
            dto.ExecutionId.Should().Be(0);
            dto.ProcessId.Should().Be(0);
            dto.StepId.Should().Be(0);
            dto.UserId.Should().BeNull();

            dto.Status.Should().NotBeNull();          // por padrão não é nulo
            dto.Status.Should().Be(string.Empty);     // default = string.Empty

            dto.StartedAt.Should().BeNull();
            dto.CompletedAt.Should().BeNull();
            dto.Remarks.Should().BeNull();
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new ProcessExecutionDto
            {
                ExecutionId = 123,
                ProcessId = 456,
                StepId = 789,
                UserId = 42,
                Status = "Running",
                StartedAt = now.AddMinutes(-5),
                CompletedAt = now,
                Remarks = "Tudo certo"
            };

            // Assert
            dto.ExecutionId.Should().Be(123);
            dto.ProcessId.Should().Be(456);
            dto.StepId.Should().Be(789);
            dto.UserId.Should().Be(42);
            dto.Status.Should().Be("Running");
            dto.StartedAt.Should().BeCloseTo(now.AddMinutes(-5), precision: TimeSpan.FromSeconds(1));
            dto.CompletedAt.Should().BeCloseTo(now, precision: TimeSpan.FromSeconds(1));
            dto.Remarks.Should().Be("Tudo certo");
        }

        [Fact]
        public void Nullable_Properties_Should_Accept_Null()
        {
            // Arrange
            var dto = new ProcessExecutionDto
            {
                UserId = null,
                StartedAt = null,
                CompletedAt = null,
                Remarks = null
            };

            // Assert
            dto.UserId.Should().BeNull();
            dto.StartedAt.Should().BeNull();
            dto.CompletedAt.Should().BeNull();
            dto.Remarks.Should().BeNull();
        }

        [Fact]
        public void Allows_Any_Timestamps_Without_Implicit_Validation()
        {
            // Arrange: CompletedAt antes de StartedAt (não há regra no DTO que proíba)
            var started = DateTime.UtcNow;
            var completed = started.AddMinutes(-10);

            var dto = new ProcessExecutionDto
            {
                StartedAt = started,
                CompletedAt = completed
            };

            // Assert: apenas verificar que os valores permanecem como setados
            dto.StartedAt.Should().Be(started);
            dto.CompletedAt.Should().Be(completed);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            // Arrange
            var original = new ProcessExecutionDto
            {
                ExecutionId = 7,
                ProcessId = 11,
                StepId = 13,
                UserId = 17,
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

            // Act
            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<ProcessExecutionDto>(json, options);

            // Assert
            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }

        // Opcional: usando AutoFixture só para garantir que atribuições aleatórias permanecem
        [Theory, AutoData]
        public void AutoFixture_Should_Assign_And_Retain_Values(
            long executionId, long processId, long stepId, long? userId,
            string status, DateTime? startedAt, DateTime? completedAt, string? remarks)
        {
            var dto = new ProcessExecutionDto
            {
                ExecutionId = executionId,
                ProcessId = processId,
                StepId = stepId,
                UserId = userId,
                Status = status ?? string.Empty,
                StartedAt = startedAt,
                CompletedAt = completedAt,
                Remarks = remarks
            };

            dto.ExecutionId.Should().Be(executionId);
            dto.ProcessId.Should().Be(processId);
            dto.StepId.Should().Be(stepId);
            dto.UserId.Should().Be(userId);
            dto.Status.Should().Be(status ?? string.Empty);
            dto.StartedAt.Should().Be(startedAt);
            dto.CompletedAt.Should().Be(completedAt);
            dto.Remarks.Should().Be(remarks);
        }
    }
}


