using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class UserEditVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new UserEditVm();

            vm.UserId.Should().Be(0);
            vm.Username.Should().Be(string.Empty);
            vm.Email.Should().BeNull();
            vm.DepartmentId.Should().BeNull();
            vm.IsActive.Should().BeFalse();    // bool default
            vm.PasswordHash.Should().BeNull(); // Required: deve ser preenchido
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new UserEditVm
            {
                UserId = 10,
                Username = "rodrigo",
                Email = "rodrigo@example.com",
                DepartmentId = 3,
                IsActive = true,
                PasswordHash = "hash@123"
            };

            vm.UserId.Should().Be(10);
            vm.Username.Should().Be("rodrigo");
            vm.Email.Should().Be("rodrigo@example.com");
            vm.DepartmentId.Should().Be(3);
            vm.IsActive.Should().BeTrue();
            vm.PasswordHash.Should().Be("hash@123");
        }

        [Fact]
        public void Validation_Should_Fail_When_Username_Is_Empty()
        {
            var vm = new UserEditVm
            {
                UserId = 1,
                Username = "",          // viola [Required] e [MinLength(3)]
                PasswordHash = "x"      // mantém outro campo válido
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(UserEditVm.Username)));
        }

        [Fact]
        public void Validation_Should_Fail_When_Username_Is_TooShort()
        {
            var vm = new UserEditVm
            {
                UserId = 1,
                Username = "ab",        // < 3
                PasswordHash = "x"
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(UserEditVm.Username)));
        }

        [Fact]
        public void Validation_Should_Fail_When_PasswordHash_Is_Null_With_Custom_Message()
        {
            var vm = new UserEditVm
            {
                UserId = 1,
                Username = "usuario",
                PasswordHash = null
            };

            var results = ValidateModel(vm);

            results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(UserEditVm.PasswordHash)))
                   .Which.ErrorMessage.Should().Be("Senha inicial é obrigatória na criação");
        }

        [Fact]
        public void Email_Is_Optional_But_When_Present_Must_Be_Valid()
        {
            // inválido
            var invalid = new UserEditVm
            {
                UserId = 1,
                Username = "usuario",
                PasswordHash = "x",
                Email = "sem-arroba"
            };
            var invalidResults = ValidateModel(invalid);
            invalidResults.Should().Contain(r => r.MemberNames.Contains(nameof(UserEditVm.Email)));

            // válido
            var valid = new UserEditVm
            {
                UserId = 1,
                Username = "usuario",
                PasswordHash = "x",
                Email = "user@dominio.com"
            };
            var validResults = ValidateModel(valid);
            validResults.Should().NotContain(r => r.MemberNames.Contains(nameof(UserEditVm.Email)));
        }

        [Fact]
        public void Validation_Should_Pass_When_All_Required_Are_Valid()
        {
            var vm = new UserEditVm
            {
                UserId = 7,
                Username = "usuarioOk",
                Email = null,          // opcional
                DepartmentId = null,   // opcional
                IsActive = true,
                PasswordHash = "hashValida"
            };

            var results = ValidateModel(vm);
            results.Should().BeEmpty();
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new UserEditVm
            {
                UserId = 77,
                Username = "admin",
                Email = "admin@empresa.com",
                DepartmentId = 9,
                IsActive = true,
                PasswordHash = "hash@xyz"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<UserEditVm>(json, options);

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
