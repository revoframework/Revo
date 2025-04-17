﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Revo.Core.Types
{
    public class VersionedTypeRegistry(ITypeIndexer typeIndexer) : IVersionedTypeRegistry
    {
        private readonly ITypeIndexer typeIndexer = typeIndexer;
        private readonly ConcurrentDictionary<Type, SubtypeRegistry> registries =
            new ConcurrentDictionary<Type, SubtypeRegistry>();

        public IEnumerable<VersionedType> GetAllTypes<TBase>() => GetSubtypeRegistry<TBase>().Ids.Values;

        public VersionedType GetTypeInfo<TBase>(VersionedTypeId id)
        {
            var registry = GetSubtypeRegistry<TBase>();
            if (!registry.Ids.TryGetValue(id, out VersionedType info))
            {
                throw new ArgumentException($"Cannot find {typeof(TBase).Name} versioned type: {id}");
            }

            return info;
        }

        public VersionedType GetTypeInfo<TBase>(Type type)
        {
            var registry = GetSubtypeRegistry<TBase>();
            if (!registry.Types.TryGetValue(type, out VersionedType info))
            {
                throw new ArgumentException($"Cannot find {typeof(TBase).Name} versioned type for CLR type: {type}");
            }

            return info;
        }

        public IReadOnlyCollection<VersionedType> GetTypeVersions<TBase>(string name)
        {
            var registry = GetSubtypeRegistry<TBase>();
            if (!registry.TypeVersions.TryGetValue(name, out var typeVersions))
            {
                throw new ArgumentException($"Cannot find {typeof(TBase).Name} type versions for type name: {name}");
            }

            return typeVersions;
        }

        public void ClearCache<TBase>() => registries.TryRemove(typeof(TBase), out var _);

        private SubtypeRegistry GetSubtypeRegistry<TBase>() =>
            registries.GetOrAdd(typeof(TBase), type =>
            {
                var ids = typeIndexer.IndexTypes<TBase>().ToImmutableDictionary(x => x.Id, x => x);
                var types = ids.Values.ToImmutableDictionary(x => x.ClrType, x => x);
                var typeVersions = ids.Values.GroupBy(x => x.Id.Name)
                    .ToImmutableDictionary(x => x.Key, x => x.ToImmutableList());

                return new SubtypeRegistry(ids, types, typeVersions);
            });

        private class SubtypeRegistry(ImmutableDictionary<VersionedTypeId, VersionedType> ids,
            ImmutableDictionary<Type, VersionedType> types,
            ImmutableDictionary<string, ImmutableList<VersionedType>> typeVersions)
        {
            public ImmutableDictionary<VersionedTypeId, VersionedType> Ids { get; } = ids;
            public ImmutableDictionary<Type, VersionedType> Types { get; } = types;
            public ImmutableDictionary<string, ImmutableList<VersionedType>> TypeVersions { get; } = typeVersions;
        }
    }
}
