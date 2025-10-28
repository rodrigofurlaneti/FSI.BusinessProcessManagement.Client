using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class UserCreateVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new UserCreateVm();

            vm.Username.Should().Be(string.Empty);
            vm.Email.Should().BeNull();
            vm.DepartmentId.Should().BeNull();
            vm.IsActive.Should().BeFalse();      // bool default
            vm.PasswordHash.Should().BeNull();   // Required: deve ser preenchido
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new UserCreateVm
            {
                Username = "rodrigo",
                Email = "rodrigo@example.com",
                DepartmentId = 10,
                IsActive = true,
                PasswordHash = "senha@hash"
            };

            vm.Username.Should().Be("rodrigo");
            vm.Email.Should().Be("rodrigo@example.com");
            vm.DepartmentId.Should().Be(10);
            vm.IsActive.Should().BeTrue();
            vm.PasswordHash.Should().Be("senha@hash");
        }

        [Fact]
        public void Validation_Should_Fail_When_Username_Is_Empty()
        {
            var vm = new UserCreateVm
            {
                Username = "",
                PasswordHash = "x" // pra falhar só por Username
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(UserCreateVm.Username)));
        }

        [Fact]
        public void Validation_Should_Fail_When_Username_Is_TooShort()
        {
            var vm = new UserCreateVm
            {
                Username = "ab",  // < 3
                PasswordHash = "x"
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(UserCreateVm.Username)));
        }

        [Fact]
        public void Validation_Should_Fail_When_PasswordHash_Is_Null_With_Custom_Message()
        {
            var vm = new UserCreateVm
            {
                Username = "validUser",
                PasswordHash = null
            };

            var results = ValidateModel(vm);

            results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(UserCreateVm.PasswordHash)))
                   .Which.ErrorMessage.Should().Be("Senha inicial é obrigatória na criação");
        }

        [Fact]
        public void Email_Is_Optional_But_When_Present_Must_Be_Valid()
        {
            // Email inválido → falha
            var invalid = new UserCreateVm
            {
                Username = "usuario",
                PasswordHash = "x",
                Email = "invalido-sem-arroba"
            };
            var invalidResults = ValidateModel(invalid);
            invalidResults.Should().Contain(r => r.MemberNames.Contains(nameof(UserCreateVm.Email)));

            // Email válido → ok
            var valid = new UserCreateVm
            {
                Username = "usuario",
                PasswordHash = "x",
                Email = "user@dominio.com"
            };
            var validResults = ValidateModel(valid);
            validResults.Should().NotContain(r => r.MemberNames.Contains(nameof(UserCreateVm.Email)));
        }

        [Fact]
        public void Validation_Should_Pass_When_All_Required_Are_Valid()
        {
            var vm = new UserCreateVm
            {
                Username = "usuarioOk",
                Email = null,          // opcional
                DepartmentId = null,   // opcional
                IsActive = false,
                PasswordHash = "hashValida"
            };

            var results = ValidateModel(vm);
            results.Should().BeEmpty();
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new UserCreateVm
            {
                Username = "admin",
                Email = "admin@empresa.com",
                DepartmentId = 7,
                IsActive = true,
                PasswordHash = "hash@123"
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<UserCreateVm>(json, options);

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
