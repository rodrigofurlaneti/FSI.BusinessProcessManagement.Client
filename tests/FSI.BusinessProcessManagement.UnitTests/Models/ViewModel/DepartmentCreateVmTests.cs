using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class DepartmentCreateVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new DepartmentCreateVm();

            vm.DepartmentName.Should().Be(string.Empty);
            vm.Description.Should().BeNull();
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new DepartmentCreateVm
            {
                DepartmentName = "Tecnologia da Informação",
                Description = "Responsável pelos sistemas internos"
            };

            vm.DepartmentName.Should().Be("Tecnologia da Informação");
            vm.Description.Should().Be("Responsável pelos sistemas internos");
        }

        [Fact]
        public void Validation_Should_Pass_When_DepartmentName_Is_Valid()
        {
            var valid = new DepartmentCreateVm { DepartmentName = "RH" + "X" };
            var results = ValidateModel(valid);

            results.Should().BeEmpty();
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new DepartmentCreateVm
            {
                DepartmentName = "Financeiro",
                Description = "Controle orçamentário"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<DepartmentCreateVm>(json, options);

            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
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
