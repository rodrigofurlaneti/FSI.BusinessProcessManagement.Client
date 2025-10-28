using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class DepartmentEditVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new DepartmentEditVm();

            vm.DepartmentId.Should().Be(0);
            vm.DepartmentName.Should().Be(string.Empty);
            vm.Description.Should().BeNull();
        }

        public void Should_Have_ValidationAttributes()
        {
            var prop = typeof(DepartmentEditVm).GetProperty(nameof(DepartmentEditVm.DepartmentName));
            prop.Should().NotBeNull();

            var requiredAttr = prop!.GetCustomAttributes(typeof(RequiredAttribute), false);
            var minLengthAttr = prop.GetCustomAttributes(typeof(MinLengthAttribute), false);

            requiredAttr.Should().NotBeEmpty("DepartmentName deve ter [Required]");
            minLengthAttr.Should().NotBeEmpty("DepartmentName deve ter [MinLength]");
            ((MinLengthAttribute)minLengthAttr[0]).Length.Should().Be(3);
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new DepartmentEditVm
            {
                DepartmentId = 10,
                DepartmentName = "Financeiro",
                Description = "Controle de custos"
            };

            vm.DepartmentId.Should().Be(10);
            vm.DepartmentName.Should().Be("Financeiro");
            vm.Description.Should().Be("Controle de custos");
        }

        [Fact]
        public void Validation_Should_Pass_When_All_Properties_Are_Valid()
        {
            var vm = new DepartmentEditVm
            {
                DepartmentId = 1,
                DepartmentName = "Tecnologia",
                Description = "Área de TI"
            };

            var results = ValidateModel(vm);
            results.Should().BeEmpty();
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new DepartmentEditVm
            {
                DepartmentId = 22,
                DepartmentName = "Recursos Humanos",
                Description = "Responsável pela gestão de pessoas"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<DepartmentEditVm>(json, options);

            copy.Should().NotBeNull();
            copy!.Should().BeEquivalentTo(original);
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }
    }
}
