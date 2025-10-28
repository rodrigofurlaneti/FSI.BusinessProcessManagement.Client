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
    public sealed class RoleDtoTests
    {
        private readonly ITestOutputHelper _output;
        public RoleDtoTests(ITestOutputHelper output) => _output = output;

        // Estruturas para “armazenar” metadados durante o teste
        private sealed record PropertyMeta(
            string Name,
            Type Type,
            bool IsNullableRef, // nulabilidade para ref types (via NullabilityInfoContext)
            bool CanRead,
            bool CanWrite);

        private sealed record TypeMeta(
            string Namespace,
            string Name,
            bool IsPublic,
            bool IsSealed,
            IReadOnlyList<PropertyMeta> Properties);

        private static Type TargetType => typeof(RoleDto);

        [Fact(DisplayName = "Forma da classe: namespace, nome, público, sealed, ctor padrão")]
        public void ClassShape_IsAsExpected()
        {
            var t = TargetType;

            Assert.Equal("FSI.BusinessProcessManagement.Models.Dto", t.Namespace);
            Assert.Equal("RoleDto", t.Name);
            Assert.True(t.IsPublic);
            Assert.True(t.IsSealed);
            Assert.NotNull(t.GetConstructor(Type.EmptyTypes));

            _output.WriteLine($"Classe: {t.FullName} | Pública: {t.IsPublic} | Sealed: {t.IsSealed}");
        }

        [Fact(DisplayName = "Propriedades: nomes, tipos, nulabilidade e acessores")]
        public void Properties_AreAsExpected_WithNullability()
        {
            var t = TargetType;
            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .OrderBy(p => p.Name, StringComparer.Ordinal)
                         .ToList();

            var nullCtx = new NullabilityInfoContext();

            var actual = props.Select(p =>
            {
                var ni = nullCtx.Create(p);
                bool isNullableRef =
                    !p.PropertyType.IsValueType &&
                    (ni.ReadState == NullabilityState.Nullable || ni.WriteState == NullabilityState.Nullable);

                return new PropertyMeta(
                    Name: p.Name,
                    Type: p.PropertyType,
                    IsNullableRef: isNullableRef,
                    CanRead: p.CanRead,
                    CanWrite: p.CanWrite
                );
            }).ToList();

            var typeMeta = new TypeMeta(
                Namespace: t.Namespace!,
                Name: t.Name,
                IsPublic: t.IsPublic,
                IsSealed: t.IsSealed,
                Properties: actual
            );

            var expected = new (string Name, Type Type, bool IsNullableRef)[]
            {
                ("Description", typeof(string), true ),  // string?
                ("RoleId",      typeof(long),   false),
                ("RoleName",    typeof(string), false),  // string não-nulo
            }.OrderBy(x => x.Name, StringComparer.Ordinal).ToArray();

            Assert.Equal(expected.Length, actual.Count);

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

        [Fact(DisplayName = "Valores padrão: RoleId=0, RoleName=\"\", Description=null")]
        public void DefaultValues_AreAsExpected()
        {
            var dto = new RoleDto();

            Assert.Equal(0L, dto.RoleId);
            Assert.Equal(string.Empty, dto.RoleName);
            Assert.Null(dto.Description);
        }

        [Fact(DisplayName = "Get/Set: aceita valores típicos e casos de borda")]
        public void GetSet_Behavior_IsCorrect()
        {
            var dto = new RoleDto
            {
                RoleId = 1,
                RoleName = "Aprovador",
                Description = "Pode aprovar etapas"
            };

            Assert.Equal(1L, dto.RoleId);
            Assert.Equal("Aprovador", dto.RoleName);
            Assert.Equal("Pode aprovar etapas", dto.Description);

            // Borda: extremos/zero e strings vazias
            dto.RoleId = long.MaxValue;
            Assert.Equal(long.MaxValue, dto.RoleId);

            dto.RoleId = 0;
            Assert.Equal(0L, dto.RoleId);

            dto.RoleName = " ";
            Assert.Equal(" ", dto.RoleName);

            dto.Description = null;
            Assert.Null(dto.Description);
        }

        [Fact(DisplayName = "JSON round-trip: serializa e desserializa mantendo dados")]
        public void Json_RoundTrip_Works()
        {
            var src = new RoleDto
            {
                RoleId = 999,
                RoleName = "Admin",
                Description = null
            };

            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            var json = JsonSerializer.Serialize(src, opts);
            _output.WriteLine("JSON: " + json);

            var back = JsonSerializer.Deserialize<RoleDto>(json, opts);
            Assert.NotNull(back);

            Assert.Equal(src.RoleId, back!.RoleId);
            Assert.Equal(src.RoleName, back.RoleName);
            Assert.Equal(src.Description, back.Description);
        }

        // -------- helpers --------
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
