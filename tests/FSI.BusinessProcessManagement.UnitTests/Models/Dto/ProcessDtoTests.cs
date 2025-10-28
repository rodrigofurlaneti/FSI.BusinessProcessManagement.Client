#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;
using FSI.BusinessProcessManagement.Models.Dto;

namespace FSI.BusinessProcessManagement.Tests.Models.Dto
{
    public sealed class ProcessDtoTests
    {
        private readonly ITestOutputHelper _output;
        public ProcessDtoTests(ITestOutputHelper output) => _output = output;

        // “Estruturas” para armazenar metadados durante o teste
        private sealed record PropertyMeta(
            string Name,
            Type Type,
            bool IsNullableRef, // para referências (via NullabilityInfoContext)
            bool CanRead,
            bool CanWrite);

        private sealed record TypeMeta(
            string Namespace,
            string Name,
            bool IsPublic,
            bool IsSealed,
            IReadOnlyList<PropertyMeta> Properties);

        private static Type TargetType => typeof(ProcessDto);

        [Fact(DisplayName = "Forma da classe: namespace, nome, público, sealed, ctor padrão")]
        public void ClassShape_IsAsExpected()
        {
            var t = TargetType;

            Assert.Equal("FSI.BusinessProcessManagement.Models.Dto", t.Namespace);
            Assert.Equal("ProcessDto", t.Name);
            Assert.True(t.IsPublic);
            Assert.True(t.IsSealed);
            Assert.NotNull(t.GetConstructor(Type.EmptyTypes));

            _output.WriteLine($"Classe: {t.FullName} | Pública: {t.IsPublic} | Sealed: {t.IsSealed}");
        }

        [Fact(DisplayName = "Propriedades: nomes, tipos, nulabilidade e acessores")]
        public void Properties_AreAsExpected_WithNullability()
        {
            var t = TargetType;
            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var nullCtx = new NullabilityInfoContext();
            var actual = props
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p =>
                {
                    var ni = nullCtx.Create(p);
                    // Para tipos de referência: Nullable se estado for Annotated
                    var isNullableRef =
                        (!p.PropertyType.IsValueType) &&
                        (ni.ReadState == NullabilityState.Nullable || ni.WriteState == NullabilityState.Nullable);

                    return new PropertyMeta(
                        Name: p.Name,
                        Type: p.PropertyType,
                        IsNullableRef: isNullableRef,
                        CanRead: p.CanRead,
                        CanWrite: p.CanWrite
                    );
                })
                .ToList();

            var typeMeta = new TypeMeta(
                Namespace: t.Namespace!,
                Name: t.Name,
                IsPublic: t.IsPublic,
                IsSealed: t.IsSealed,
                Properties: actual
            );

            // Expectativa (nomes e tipos)
            var expected = new (string Name, Type Type, bool IsNullableRef)[]
            {
                ("ProcessId",   typeof(long),   false),
                ("ProcessName", typeof(string), false), // anotado como não-nulo no código
                ("DepartmentId",typeof(long),   false),
                ("Description", typeof(string), false), // anotado como não-nulo no código
                ("CreatedBy",   typeof(long),   false),
            };

            // Quantidade
            Assert.Equal(expected.Length, actual.Count);

            // Nomes/Tipos/Nulabilidade & RW
            foreach (var (Name, Type, IsNullableRef) in expected)
            {
                var p = actual.SingleOrDefault(x => x.Name == Name);
                Assert.NotNull(p);
                Assert.Equal(Type, p!.Type);
                Assert.Equal(IsNullableRef, p.IsNullableRef);
                Assert.True(p.CanRead);
                Assert.True(p.CanWrite);
            }

            // Log/auditoria
            _output.WriteLine($"[META] {typeMeta.Namespace}.{typeMeta.Name}");
            foreach (var p in typeMeta.Properties)
                _output.WriteLine($"  - {p.Name}: {Pretty(p.Type)} | Nullable={(p.IsNullableRef ? "Yes" : "No")} | R/W={p.CanRead}/{p.CanWrite}");
        }

        [Fact(DisplayName = "Valores padrão: longs = 0, ProcessName = \"\", Description = null (atenção!)")]
        public void DefaultValues_AreAsExpected()
        {
            var dto = new ProcessDto();

            // longs padrão = 0
            Assert.Equal(0L, dto.ProcessId);
            Assert.Equal(0L, dto.DepartmentId);
            Assert.Equal(0L, dto.CreatedBy);

            // ProcessName tem inicializador = ""
            Assert.Equal(string.Empty, dto.ProcessName);

            // Description é 'string' (não-nulo) mas sem inicializador → runtime default é null
            Assert.Null(dto.Description);

            // Observação útil para revisão de modelo:
            _output.WriteLine("OBS: Description está anotado como não-nulo, mas inicia null em runtime. Considere `string?` ou inicializar com string.Empty.");
        }

        [Fact(DisplayName = "Get/Set: aceita valores típicos e casos de borda")]
        public void GetSet_Behavior_IsCorrect()
        {
            var dto = new ProcessDto
            {
                ProcessId = 10,
                ProcessName = "Onboarding",
                DepartmentId = 5,
                Description = "Fluxo de entrada",
                CreatedBy = 42
            };

            Assert.Equal(10L, dto.ProcessId);
            Assert.Equal("Onboarding", dto.ProcessName);
            Assert.Equal(5L, dto.DepartmentId);
            Assert.Equal("Fluxo de entrada", dto.Description);
            Assert.Equal(42L, dto.CreatedBy);

            // Borda: valores grandes/zero
            dto.ProcessId = long.MaxValue;
            dto.DepartmentId = 0;
            dto.CreatedBy = long.MinValue + 1; // ainda válido enquanto long

            Assert.Equal(long.MaxValue, dto.ProcessId);
            Assert.Equal(0L, dto.DepartmentId);
            Assert.Equal(long.MinValue + 1, dto.CreatedBy);

            // Borda: strings
            dto.ProcessName = " "; // permitido (DTO não impõe validação de domínio)
            Assert.Equal(" ", dto.ProcessName);

            // Atribuição null em Description (gera warning em compile se #nullable enable, mas o teste pode validar o comportamento de runtime)
#pragma warning disable CS8625
            dto.Description = null;
#pragma warning restore CS8625
            Assert.Null(dto.Description);
        }

        [Fact(DisplayName = "JSON round-trip: serializa e desserializa mantendo dados")]
        public void Json_RoundTrip_Works()
        {
            var src = new ProcessDto
            {
                ProcessId = 77,
                ProcessName = "Cadastro de Fornecedores",
                DepartmentId = 3,
                Description = "Fluxo de aprovação de fornecedores",
                CreatedBy = 1001
            };

            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            var json = JsonSerializer.Serialize(src, opts);
            _output.WriteLine("JSON: " + json);

            var back = JsonSerializer.Deserialize<ProcessDto>(json, opts);
            Assert.NotNull(back);

            Assert.Equal(src.ProcessId, back!.ProcessId);
            Assert.Equal(src.ProcessName, back.ProcessName);
            Assert.Equal(src.DepartmentId, back.DepartmentId);
            Assert.Equal(src.Description, back.Description);
            Assert.Equal(src.CreatedBy, back.CreatedBy);
        }

        // -------- helpers --------
        private static string Pretty(Type t)
        {
            if (!t.IsGenericType) return t.Name switch
            {
                "Int64" => "long",
                "String" => "string",
                "Int32" => "int",
                _ => t.Name
            };

            var gtd = t.GetGenericTypeDefinition();
            var args = string.Join(", ", t.GetGenericArguments().Select(Pretty));
            var name = gtd.Name[..gtd.Name.IndexOf('`')];
            return $"{name}<{args}>";
        }
    }
}
