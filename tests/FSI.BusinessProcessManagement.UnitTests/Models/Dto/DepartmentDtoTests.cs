#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;
using FSI.BusinessProcessManagement.Models.Dto;

namespace FSI.BusinessProcessManagement.Tests.Models.Dto
{
    public sealed class DepartmentDtoTests
    {
        private readonly ITestOutputHelper _output;

        public DepartmentDtoTests(ITestOutputHelper output) => _output = output;

        // Estruturas auxiliares para "armazenar" metadados durante o teste
        private sealed record PropertyMeta(
            string Name,
            Type Type,
            bool IsNullable,     // para ref types via NullabilityInfoContext
            bool CanRead,
            bool CanWrite);

        private sealed record TypeMeta(
            string Namespace,
            string Name,
            bool IsPublic,
            bool IsSealed,
            IReadOnlyList<PropertyMeta> Properties);

        private static Type TargetType => typeof(DepartmentDto);

        [Fact(DisplayName = "Forma da classe: namespace, nome, público, sealed, ctor padrão")]
        public void ClassShape_IsAsExpected()
        {
            // Arrange
            var t = TargetType;

            // Act
            var hasParameterlessCtor = t.GetConstructor(Type.EmptyTypes) is not null;

            // Assert
            Assert.Equal("FSI.BusinessProcessManagement.Models.Dto", t.Namespace);
            Assert.Equal("DepartmentDto", t.Name);
            Assert.True(t.IsPublic);
            Assert.True(t.IsSealed);
            Assert.True(hasParameterlessCtor);

            // Armazena/mostra metadados básicos
            _output.WriteLine($"Classe: {t.FullName}");
            _output.WriteLine($"Pública: {t.IsPublic}, Sealed: {t.IsSealed}, Ctor padrão: {hasParameterlessCtor}");
        }

        [Fact(DisplayName = "Propriedades: nomes, tipos, nulabilidade e acessores")]
        public void Properties_AreAsExpected_WithNullability()
        {
            // Arrange
            var t = TargetType;
            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var nullCtx = new NullabilityInfoContext();

            // Armazena "expectativa" — ordem aqui é apenas para comparação estável
            var expected = new[]
            {
                new {Name = "DepartmentId",   Type = typeof(long),   IsNullable = false},
                new {Name = "DepartmentName", Type = typeof(string), IsNullable = false},
                new {Name = "Description",    Type = typeof(string), IsNullable = true },
            };

            // Act
            // Monta metadados reais
            var actual = props
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p =>
                {
                    var ni = nullCtx.Create(p);
                    // Para ref types: NotAnnotated => não-nulo, Annotated => nulo
                    var isNullableRef =
                        (!p.PropertyType.IsValueType) &&
                        (ni.ReadState == NullabilityState.Nullable || ni.WriteState == NullabilityState.Nullable);

                    return new PropertyMeta(
                        Name: p.Name,
                        Type: p.PropertyType,
                        IsNullable: isNullableRef,
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

            // Assert – quantidade
            Assert.Equal(expected.Length, actual.Count);

            // Assert – nomes & tipos & nulabilidade & acessores
            foreach (var exp in expected)
            {
                var got = actual.SingleOrDefault(p => p.Name == exp.Name);
                Assert.NotNull(got);
                Assert.Equal(exp.Type, got!.Type);
                Assert.Equal(exp.IsNullable, got.IsNullable);
                Assert.True(got.CanRead);
                Assert.True(got.CanWrite);
            }

            // Loga/“armazena” os metadados para auditoria
            _output.WriteLine($"[META] {typeMeta.Namespace}.{typeMeta.Name}");
            foreach (var p in typeMeta.Properties)
                _output.WriteLine($"  - {p.Name}: {Pretty(p.Type)} | Nullable={(p.IsNullable ? "Yes" : "No")} | R/W={p.CanRead}/{p.CanWrite}");
        }

        [Fact(DisplayName = "Valores padrão: DepartmentId=0, DepartmentName=\"\", Description=null")]
        public void DefaultValues_AreAsExpected()
        {
            // Arrange
            var dto = new DepartmentDto();

            // Assert
            Assert.Equal(0L, dto.DepartmentId);
            Assert.Equal(string.Empty, dto.DepartmentName);
            Assert.Null(dto.Description);
        }

        [Fact(DisplayName = "Get/Set: aceita valores típicos e casos de borda")]
        public void GetSet_Behavior_IsCorrect()
        {
            var dto = new DepartmentDto
            {
                DepartmentId = 123,
                DepartmentName = "Financeiro",
                Description = "Centro de custos"
            };

            Assert.Equal(123L, dto.DepartmentId);
            Assert.Equal("Financeiro", dto.DepartmentName);
            Assert.Equal("Centro de custos", dto.Description);

            // Casos de borda
            dto.DepartmentId = long.MaxValue;
            Assert.Equal(long.MaxValue, dto.DepartmentId);

            dto.DepartmentId = 0; // permitido (sem validação de domínio aqui)
            Assert.Equal(0L, dto.DepartmentId);

            dto.DepartmentName = " "; // permitido (o tipo não impõe regras de domínio)
            Assert.Equal(" ", dto.DepartmentName);

            dto.Description = null; // opcional
            Assert.Null(dto.Description);
        }

        [Fact(DisplayName = "JSON round-trip: serializa e desserializa mantendo dados")]
        public void Json_RoundTrip_Works()
        {
            var src = new DepartmentDto
            {
                DepartmentId = 987,
                DepartmentName = "TI",
                Description = "Infraestrutura e Suporte"
            };

            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            var json = JsonSerializer.Serialize(src, opts);
            _output.WriteLine("JSON: " + json);

            var back = JsonSerializer.Deserialize<DepartmentDto>(json, opts);
            Assert.NotNull(back);

            Assert.Equal(src.DepartmentId, back!.DepartmentId);
            Assert.Equal(src.DepartmentName, back.DepartmentName);
            Assert.Equal(src.Description, back.Description);
        }

        // ----------------- helpers -----------------
        private static string Pretty(Type t)
        {
            if (!t.IsGenericType) return t.Name switch
            {
                "Int64" => "long",
                "String" => "string",
                _ => t.Name
            };

            var gtd = t.GetGenericTypeDefinition();
            var args = string.Join(", ", t.GetGenericArguments().Select(Pretty));
            var name = gtd.Name[..gtd.Name.IndexOf('`')];
            return $"{name}<{args}>";
        }
    }
}
