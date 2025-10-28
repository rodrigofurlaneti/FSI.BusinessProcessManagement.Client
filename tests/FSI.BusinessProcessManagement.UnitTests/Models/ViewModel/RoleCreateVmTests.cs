using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class RoleCreateVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new RoleCreateVm();

            vm.RoleName.Should().Be(string.Empty);
            vm.Description.Should().BeNull();
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new RoleCreateVm
            {
                RoleName = "Administrador",
                Description = "Acesso total ao sistema"
            };

            vm.RoleName.Should().Be("Administrador");
            vm.Description.Should().Be("Acesso total ao sistema");
        }

        [Fact]
        public void Validation_Should_Fail_When_RoleName_Is_Empty()
        {
            var vm = new RoleCreateVm
            {
                RoleName = "" // viola [Required] e [MinLength(3)]
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(RoleCreateVm.RoleName)));
        }

        [Fact]
        public void Validation_Should_Fail_When_RoleName_Is_TooShort()
        {
            var vm = new RoleCreateVm
            {
                RoleName = "ab" // < 3
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(RoleCreateVm.RoleName)));
        }

        [Fact]
        public void Validation_Should_Pass_When_Valid()
        {
            var vm = new RoleCreateVm
            {
                RoleName = "Gestor",
                Description = null // opcional
            };

            var results = ValidateModel(vm);

            results.Should().BeEmpty();
        }

        [Fact]
        public void RoleName_Should_Have_Required_And_MinLength_Attributes()
        {
            var prop = typeof(RoleCreateVm).GetProperty(nameof(RoleCreateVm.RoleName));
            prop.Should().NotBeNull();

            var required = prop!.GetCustomAttributes(typeof(RequiredAttribute), inherit: false);
            var minLength = prop.GetCustomAttributes(typeof(MinLengthAttribute), inherit: false);

            required.Should().NotBeEmpty("[Required] deve estar aplicado em RoleName");
            minLength.Should().NotBeEmpty("[MinLength] deve estar aplicado em RoleName");
            ((MinLengthAttribute)minLength[0]).Length.Should().Be(3);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new RoleCreateVm
            {
                RoleName = "Operador",
                Description = "Pode operar módulos específicos"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<RoleCreateVm>(json, options);

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
