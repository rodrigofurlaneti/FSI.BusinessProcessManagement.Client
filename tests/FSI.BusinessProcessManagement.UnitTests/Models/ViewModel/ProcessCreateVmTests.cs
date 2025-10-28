using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class ProcessCreateVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new ProcessCreateVm();

            vm.ProcessName.Should().Be(string.Empty);
            vm.DepartmentId.Should().Be(0);   // falha em [Range] até ser setado
            vm.Description.Should().BeNull();
            vm.CreatedBy.Should().Be(0);
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new ProcessCreateVm
            {
                ProcessName = "Aprovação de Faturas",
                DepartmentId = 10,
                Description = "Fluxo de aprovação e pagamento",
                CreatedBy = 99
            };

            vm.ProcessName.Should().Be("Aprovação de Faturas");
            vm.DepartmentId.Should().Be(10);
            vm.Description.Should().Be("Fluxo de aprovação e pagamento");
            vm.CreatedBy.Should().Be(99);
        }

        [Fact]
        public void Validation_Should_Fail_When_ProcessName_Is_Empty()
        {
            var vm = new ProcessCreateVm
            {
                ProcessName = "",      // Required
                DepartmentId = 1,      // válido
                CreatedBy = 1
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(ProcessCreateVm.ProcessName)))
                   .And.ContainSingle(r => r.ErrorMessage == "Informe o nome");
        }

        [Fact]
        public void Validation_Should_Fail_When_DepartmentId_Is_Less_Than_1()
        {
            var vm = new ProcessCreateVm
            {
                ProcessName = "Cadastro de Fornecedor",
                DepartmentId = 0,      // viola [Range(1, long.MaxValue)]
                CreatedBy = 1
            };

            var results = ValidateModel(vm);

            results.Should().Contain(r => r.MemberNames.Contains(nameof(ProcessCreateVm.DepartmentId)))
                   .And.ContainSingle(r => r.ErrorMessage == "Selecione um departamento");
        }

        [Fact]
        public void Validation_Should_Pass_When_All_Required_Are_Valid()
        {
            var vm = new ProcessCreateVm
            {
                ProcessName = "Solicitação de Compra",
                DepartmentId = 3,     // dentro do range
                Description = null,   // opcional
                CreatedBy = 7
            };

            var results = ValidateModel(vm);

            results.Should().BeEmpty("todos os campos obrigatórios são válidos");
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new ProcessCreateVm
            {
                ProcessName = "Onboarding",
                DepartmentId = 5,
                Description = "Processo de integração",
                CreatedBy = 123
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<ProcessCreateVm>(json, options);

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
