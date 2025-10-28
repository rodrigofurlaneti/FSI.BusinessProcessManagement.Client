using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class ProcessStepCreateVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new ProcessStepCreateVm();

            vm.ProcessId.Should().BeNull();              // nullable
            vm.StepName.Should().Be(string.Empty);       // default
            vm.StepOrder.Should().Be(0);                 // default int
            vm.AssignedRoleId.Should().BeNull();         // nullable
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new ProcessStepCreateVm
            {
                ProcessId = 10,
                StepName = "Validação Inicial",
                StepOrder = 2,
                AssignedRoleId = 99
            };

            vm.ProcessId.Should().Be(10);
            vm.StepName.Should().Be("Validação Inicial");
            vm.StepOrder.Should().Be(2);
            vm.AssignedRoleId.Should().Be(99);
        }

        [Fact]
        public void Validation_Should_Fail_When_StepName_Is_Empty()
        {
            var vm = new ProcessStepCreateVm
            {
                StepName = "",     // viola [Required] + [MinLength(3)]
                StepOrder = 1
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(ProcessStepCreateVm.StepName)));
        }

        [Fact]
        public void Validation_Should_Fail_When_StepName_Is_TooShort()
        {
            var vm = new ProcessStepCreateVm
            {
                StepName = "ab",   // < 3
                StepOrder = 1
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(ProcessStepCreateVm.StepName)));
        }

        [Fact]
        public void Validation_Should_Pass_When_All_Required_Are_Valid()
        {
            var vm = new ProcessStepCreateVm
            {
                ProcessId = null,          // permitido (nullable)
                StepName = "Análise",
                StepOrder = 0,             // permitido (sem atributo de range)
                AssignedRoleId = null      // permitido (nullable)
            };

            var results = ValidateModel(vm);

            results.Should().BeEmpty();
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new ProcessStepCreateVm
            {
                ProcessId = 1,
                StepName = "Aprovação",
                StepOrder = 3,
                AssignedRoleId = 7
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<ProcessStepCreateVm>(json, options);

            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }

        // (Opcional) Garante que os atributos de validação estão aplicados na propriedade
        [Fact]
        public void StepName_Should_Have_Required_And_MinLength_Attributes()
        {
            var prop = typeof(ProcessStepCreateVm).GetProperty(nameof(ProcessStepCreateVm.StepName));
            prop.Should().NotBeNull();

            var required = prop!.GetCustomAttributes(typeof(RequiredAttribute), inherit: false);
            var minLength = prop.GetCustomAttributes(typeof(MinLengthAttribute), inherit: false);

            required.Should().NotBeEmpty("StepName deve ter [Required]");
            minLength.Should().NotBeEmpty("StepName deve ter [MinLength]");
            ((MinLengthAttribute)minLength[0]).Length.Should().Be(3);
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var ctx = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
            return results;
        }
    }
}
