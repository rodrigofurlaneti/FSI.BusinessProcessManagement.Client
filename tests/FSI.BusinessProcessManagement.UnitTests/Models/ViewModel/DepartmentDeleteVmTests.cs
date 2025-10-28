using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentAssertions;
using FSI.BusinessProcessManagement.Models.ViewModel;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Models.ViewModel
{
    public sealed class DepartmentDeleteVmTests
    {
        [Fact]
        public void Ctor_Should_Set_DefaultValues()
        {
            var vm = new DepartmentDeleteVm();

            vm.DepartmentId.Should().Be(0);
        }

        [Fact]
        public void Properties_Should_ReadAndWrite_Correctly()
        {
            var vm = new DepartmentDeleteVm
            {
                DepartmentId = 123
            };

            vm.DepartmentId.Should().Be(123);
        }

        [Fact]
        public void Json_Serialization_RoundTrip_Should_Preserve_Values()
        {
            var original = new DepartmentDeleteVm { DepartmentId = 55 };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(original, options);
            var copy = JsonSerializer.Deserialize<DepartmentDeleteVm>(json, options);

            copy.Should().NotBeNull();
            copy!.DepartmentId.Should().Be(55);
        }
    }

}
