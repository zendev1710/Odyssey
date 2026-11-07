using Dock.Model.Core;
using Dock.Serializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Odyssey.ViewModels;

internal sealed class LayoutDockSerializer : IDockSerializer
{
    private readonly JsonSerializerSettings _settings = new()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.Objects,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        ContractResolver = new LayoutContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new KeyValuePairConverter() }
    };

    public string Serialize<T>(T value) => JsonConvert.SerializeObject(value, _settings);

    public T? Deserialize<T>(string text) => JsonConvert.DeserializeObject<T>(text, _settings);

    public T? Load<T>(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return Deserialize<T>(reader.ReadToEnd());
    }

    public void Save<T>(Stream stream, T value)
    {
        string text = Serialize(value);
        if (!string.IsNullOrWhiteSpace(text))
        {
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(text);
        }
    }

    private sealed class LayoutContractResolver : ListContractResolver
    {
        public LayoutContractResolver() : base(typeof(ObservableCollection<>))
        {
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            List<JsonProperty> properties = base.CreateProperties(type, memberSerialization)
                .Where(property =>
                    property.PropertyName is not ("Context" or "Content" or "DataContext")
                    && property.DeclaringType?.Namespace?.StartsWith("Odyssey.ViewModels", StringComparison.Ordinal) != true)
                .ToList();

            if (typeof(IDockable).IsAssignableFrom(type))
            {
                JsonProperty? originalOwner = properties.FirstOrDefault(property =>
                    string.Equals(property.PropertyName, nameof(IDockable.OriginalOwner), StringComparison.Ordinal));
                if (originalOwner is null && type.GetProperty(nameof(IDockable.OriginalOwner)) is { } originalOwnerMember)
                {
                    originalOwner = CreateProperty(originalOwnerMember, memberSerialization);
                    properties.Add(originalOwner);
                }

                if (originalOwner is not null)
                {
                    originalOwner.Ignored = false;
                    originalOwner.Readable = true;
                    originalOwner.Writable = true;
                }
            }

            return properties;
        }
    }
}
