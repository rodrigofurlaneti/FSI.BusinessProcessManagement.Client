using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class RoleEditVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new RoleEditVm();

            vm.RoleId.Should().Be(0);
            vm.RoleName.Should().Be(string.Empty);
            vm.Description.Should().BeNull();
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new RoleEditVm
            {
                RoleId = 10,
                RoleName = "Supervisor",
                Description = "Gerencia as equipes de operação"
            };

            vm.RoleId.Should().Be(10);
            vm.RoleName.Should().Be("Supervisor");
            vm.Description.Should().Be("Gerencia as equipes de operação");
        }

        [Fact]
        public void Validation_Should_Fail_When_RoleName_Is_Empty()
        {
            var vm = new RoleEditVm
            {
                RoleName = ""
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(RoleEditVm.RoleName)));
        }

        [Fact]
        public void Validation_Should_Fail_When_RoleName_Is_TooShort()
        {
            var vm = new RoleEditVm
            {
                RoleName = "ab"
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(RoleEditVm.RoleName)));
        }

        [Fact]
        public void Validation_Should_Pass_When_RoleName_Is_Valid()
        {
            var vm = new RoleEditVm
            {
                RoleId = 5,
                RoleName = "Gestor",
                Description = "Responsável por aprovações"
            };

            var results = ValidateModel(vm);
            results.Should().BeEmpty();
        }

        [Fact]
        public void RoleName_Should_Have_Required_And_MinLength_Attributes()
        {
            var prop = typeof(RoleEditVm).GetProperty(nameof(RoleEditVm.RoleName));
            prop.Should().NotBeNull();

            var requiredAttr = prop!.GetCustomAttributes(typeof(RequiredAttribute), false);
            var minLengthAttr = prop.GetCustomAttributes(typeof(MinLengthAttribute), false);

            requiredAttr.Should().NotBeEmpty("[Required] deve estar aplicado em RoleName");
            minLengthAttr.Should().NotBeEmpty("[MinLength] deve estar aplicado em RoleName");
            ((MinLengthAttribute)minLengthAttr[0]).Length.Should().Be(3);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new RoleEditVm
            {
                RoleId = 1,
                RoleName = "Administrador",
                Description = "Acesso completo"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<RoleEditVm>(json, options);

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
