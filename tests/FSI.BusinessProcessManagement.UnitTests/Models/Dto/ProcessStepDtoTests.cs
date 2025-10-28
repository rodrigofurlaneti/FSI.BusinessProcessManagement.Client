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
    public sealed class ProcessStepDtoTests
    {
        private readonly ITestOutputHelper _output;
        public ProcessStepDtoTests(ITestOutputHelper output) => _output = output;

        // -------- estruturas para "armazenar" metadados --------
        private sealed record PropertyMeta(
            string Name,
            Type Type,
            bool IsNullableRef,   // string, etc (via NullabilityInfoContext)
            bool IsNullableValue, // Nullable<T> para value types (long?, int?, etc.)
            bool CanRead,
            bool CanWrite);

        private sealed record TypeMeta(
            string Namespace,
            string Name,
            bool IsPublic,
            bool IsSealed,
            IReadOnlyList<PropertyMeta> Properties);

        private static Type TargetType => typeof(ProcessStepDto);

        [Fact(DisplayName = "Forma da classe: namespace, nome, público, sealed, ctor padrão")]
        public void ClassShape_IsAsExpected()
        {
            var t = TargetType;

            Assert.Equal("FSI.BusinessProcessManagement.Models.Dto", t.Namespace);
            Assert.Equal("ProcessStepDto", t.Name);
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

            // Real → coleta metadados
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

            // Expectativa (nomes, tipos e nulabilidade)
            var expected = new (string Name, Type Type, bool IsNullableRef, bool IsNullableValue)[]
            {
                ("AssignedRoleId", typeof(long?),  false, true ),
                ("ProcessId",      typeof(long),   false, false),
                ("StepId",         typeof(long),   false, false),
                ("StepName",       typeof(string), false, false),
                ("StepOrder",      typeof(int),    false, false),
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

            // Log/auditoria dos metadados (classe, propriedades e tipos)
            _output.WriteLine($"[META] {typeMeta.Namespace}.{typeMeta.Name}");
            foreach (var p in typeMeta.Properties)
                _output.WriteLine($"  - {p.Name}: {Pretty(p.Type)} | NullableRef={(p.IsNullableRef ? "Yes" : "No")} | NullableVal={(p.IsNullableValue ? "Yes" : "No")} | R/W={p.CanRead}/{p.CanWrite}");
        }

        [Fact(DisplayName = "Valores padrão: StepId/ProcessId/StepOrder=0, StepName=\"\", AssignedRoleId=null")]
        public void DefaultValues_AreAsExpected()
        {
            var dto = new ProcessStepDto();

            Assert.Equal(0L, dto.StepId);
            Assert.Equal(0L, dto.ProcessId);
            Assert.Equal(0, dto.StepOrder);
            Assert.Equal(string.Empty, dto.StepName);
            Assert.Null(dto.AssignedRoleId);
        }

        [Fact(DisplayName = "Get/Set: aceita valores típicos e casos de borda")]
        public void GetSet_Behavior_IsCorrect()
        {
            var dto = new ProcessStepDto
            {
                StepId = 1,
                ProcessId = 10,
                StepName = "Aprovação",
                StepOrder = 2,
                AssignedRoleId = 99
            };

            Assert.Equal(1L, dto.StepId);
            Assert.Equal(10L, dto.ProcessId);
            Assert.Equal("Aprovação", dto.StepName);
            Assert.Equal(2, dto.StepOrder);
            Assert.Equal(99L, dto.AssignedRoleId);

            // Bordas: extremos e nulos
            dto.StepId = long.MaxValue;
            dto.ProcessId = long.MinValue + 1; // ainda é long válido
            dto.StepOrder = int.MaxValue;
            dto.StepName = " "; // DTO não impõe validação, então é permitido
            dto.AssignedRoleId = null;

            Assert.Equal(long.MaxValue, dto.StepId);
            Assert.Equal(long.MinValue + 1, dto.ProcessId);
            Assert.Equal(int.MaxValue, dto.StepOrder);
            Assert.Equal(" ", dto.StepName);
            Assert.Null(dto.AssignedRoleId);
        }

        [Fact(DisplayName = "JSON round-trip: serializa e desserializa mantendo dados")]
        public void Json_RoundTrip_Works()
        {
            var src = new ProcessStepDto
            {
                StepId = 321,
                ProcessId = 654,
                StepName = "Revisão",
                StepOrder = 3,
                AssignedRoleId = null
            };

            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            var json = JsonSerializer.Serialize(src, opts);
            _output.WriteLine("JSON: " + json);

            var back = JsonSerializer.Deserialize<ProcessStepDto>(json, opts);
            Assert.NotNull(back);

            Assert.Equal(src.StepId, back!.StepId);
            Assert.Equal(src.ProcessId, back.ProcessId);
            Assert.Equal(src.StepName, back.StepName);
            Assert.Equal(src.StepOrder, back.StepOrder);
            Assert.Equal(src.AssignedRoleId, back.AssignedRoleId);
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
                "Int32" => "int",
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
