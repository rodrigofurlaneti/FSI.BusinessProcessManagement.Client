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
    public sealed class UserDtoTests
    {
        private readonly ITestOutputHelper _output;
        public UserDtoTests(ITestOutputHelper output) => _output = output;

        // Estruturas para armazenar metadados durante o teste
        private sealed record PropertyMeta(
            string Name,
            Type Type,
            bool IsNullableRef,   // string, etc. (via NullabilityInfoContext)
            bool IsNullableValue, // Nullable<T> para value types (long?, bool?)
            bool CanRead,
            bool CanWrite);

        private sealed record TypeMeta(
            string Namespace,
            string Name,
            bool IsPublic,
            bool IsSealed,
            IReadOnlyList<PropertyMeta> Properties);

        private static Type TargetType => typeof(UserDto);

        [Fact(DisplayName = "Forma da classe: namespace, público, sealed e ctor padrão")]
        public void ClassShape_IsAsExpected()
        {
            var t = TargetType;

            Assert.Equal("FSI.BusinessProcessManagement.Models.Dto", t.Namespace);
            Assert.Equal("UserDto", t.Name);
            Assert.True(t.IsPublic);
            Assert.True(t.IsSealed);
            Assert.NotNull(t.GetConstructor(Type.EmptyTypes));

            _output.WriteLine($"Classe: {t.FullName} | Pública: {t.IsPublic} | Sealed: {t.IsSealed}");
        }

        [Fact(DisplayName = "Propriedades: nomes, tipos, nulabilidade (ref e value types) e acessores")]
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
                bool isRef = !p.PropertyType.IsValueType;
                bool isNullableRef =
                    isRef && (ni.ReadState == NullabilityState.Nullable || ni.WriteState == NullabilityState.Nullable);
                bool isNullableValue = Nullable.GetUnderlyingType(p.PropertyType) is not null;

                return new PropertyMeta(
                    Name: p.Name,
                    Type: p.PropertyType,
                    IsNullableRef: isNullableRef,
                    IsNullableValue: isNullableValue,
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

            var expected = new (string Name, Type Type, bool IsNullableRef, bool IsNullableValue)[]
            {
                ("DepartmentId", typeof(long?),  false, true ),
                ("Email",        typeof(string), true,  false),
                ("IsActive",     typeof(bool),   false, false),
                ("PasswordHash", typeof(string), true,  false),
                ("UserId",       typeof(long),   false, false),
                ("Username",     typeof(string), false, false),
            }.OrderBy(x => x.Name, StringComparer.Ordinal).ToArray();

            Assert.Equal(expected.Length, actual.Count);

            foreach (var e in expected)
            {
                var p = actual.SingleOrDefault(x => x.Name == e.Name);
                Assert.NotNull(p);
                Assert.Equal(e.Type, p!.Type);
                Assert.Equal(e.IsNullableRef, p.IsNullableRef);
                Assert.Equal(e.IsNullableValue, p.IsNullableValue);
                Assert.True(p.CanRead);
                Assert.True(p.CanWrite);
            }

            // Log/auditoria
            _output.WriteLine($"[META] {typeMeta.Namespace}.{typeMeta.Name}");
            foreach (var p in typeMeta.Properties)
                _output.WriteLine($"  - {p.Name}: {Pretty(p.Type)} | NullableRef={(p.IsNullableRef ? "Yes" : "No")} | NullableVal={(p.IsNullableValue ? "Yes" : "No")} | R/W={p.CanRead}/{p.CanWrite}");
        }

        [Fact(DisplayName = "Valores padrão: UserId=0, DepartmentId=null, Username=\"\", Email=null, IsActive=true, PasswordHash=null")]
        public void DefaultValues_AreAsExpected()
        {
            var dto = new UserDto();

            Assert.Equal(0L, dto.UserId);
            Assert.Null(dto.DepartmentId);
            Assert.Equal(string.Empty, dto.Username);
            Assert.Null(dto.Email);
            Assert.True(dto.IsActive);
            Assert.Null(dto.PasswordHash);
        }

        [Fact(DisplayName = "Get/Set: aceita valores típicos e casos de borda")]
        public void GetSet_Behavior_IsCorrect()
        {
            var dto = new UserDto
            {
                UserId = 42,
                DepartmentId = 7,
                Username = "rodrigo",
                Email = "rodrigo@empresa.com",
                IsActive = true,
                PasswordHash = "hash-abc"
            };

            Assert.Equal(42L, dto.UserId);
            Assert.Equal(7L, dto.DepartmentId);
            Assert.Equal("rodrigo", dto.Username);
            Assert.Equal("rodrigo@empresa.com", dto.Email);
            Assert.True(dto.IsActive);
            Assert.Equal("hash-abc", dto.PasswordHash);

            // Bordas/extremos
            dto.UserId = long.MaxValue;
            dto.DepartmentId = null; // permitido
            dto.Username = " ";      // DTO não valida domínio
            dto.Email = null;        // opcional
            dto.IsActive = false;    // alternando o bool
            dto.PasswordHash = null; // opcional

            Assert.Equal(long.MaxValue, dto.UserId);
            Assert.Null(dto.DepartmentId);
            Assert.Equal(" ", dto.Username);
            Assert.Null(dto.Email);
            Assert.False(dto.IsActive);
            Assert.Null(dto.PasswordHash);
        }

        [Fact(DisplayName = "JSON round-trip: serializa e desserializa mantendo dados")]
        public void Json_RoundTrip_Works()
        {
            var src = new UserDto
            {
                UserId = 1001,
                DepartmentId = null,
                Username = "alice",
                Email = null,
                IsActive = true,
                PasswordHash = null
            };

            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            var json = JsonSerializer.Serialize(src, opts);
            _output.WriteLine("JSON: " + json);

            var back = JsonSerializer.Deserialize<UserDto>(json, opts);
            Assert.NotNull(back);

            Assert.Equal(src.UserId, back!.UserId);
            Assert.Equal(src.DepartmentId, back.DepartmentId);
            Assert.Equal(src.Username, back.Username);
            Assert.Equal(src.Email, back.Email);
            Assert.Equal(src.IsActive, back.IsActive);
            Assert.Equal(src.PasswordHash, back.PasswordHash);
        }

        // -------- helpers --------
        private static string Pretty(Type t)
        {
            // Nullable<T> → "long?"
            var ut = Nullable.GetUnderlyingType(t);
            if (ut is not null) return Pretty(ut) + "?";

            if (!t.IsGenericType) return t.Name switch
            {
                "Int64" => "long",
                "Boolean" => "bool",
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
