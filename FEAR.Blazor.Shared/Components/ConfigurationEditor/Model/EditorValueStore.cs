using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FEAR.Blazor.Shared.Components.ConfigurationEditor.Model
{
    public class EditorValueStore
    {
        public Action OnValueChanged;

        public class ValueEntry
        {
            public string StoreValue { get; set; }
            public object Value
            {
                get { return DeserializeValue(StoreValue, StoredType); }
                set
                {
                    StoreValue = SerializeValue(value);

                    StoredType = value.GetType();
                }
            }

            [JsonIgnore]
            public Type StoredType { get; set; }

            public ValueEntry()
            {
            }

            public ValueEntry(object value)
            {
                Value = value;
            }

            public void UpdateValue(object value)
            {
                // Validate the types match
                if (StoredType == null || value.GetType() == StoredType)
                    Value = value;
            }

            private string SerializeValue(object value)
            {
                if (value == null)
                    return null;
                if (value is string || value.GetType().IsPrimitive)
                    return value.ToString();
                return System.Text.Json.JsonSerializer.Serialize(value);
            }

            private object DeserializeValue(string storeValue, Type storedType)
            {
                if (string.IsNullOrEmpty(storeValue))
                    return null;
                if (storedType == typeof(string))
                    return storeValue;
                if (storedType.IsAssignableFrom(typeof(int)))
                    return int.Parse(storeValue);
                if (storedType == typeof(bool))
                    return bool.Parse(storeValue);
                if (storedType.IsAssignableFrom(typeof(double)))
                    return double.Parse(storeValue);
                // Add more primitive types as needed
                return System.Text.Json.JsonSerializer.Deserialize(storeValue, storedType);
            }
        }

        public Dictionary<string, ValueEntry> Values { get; set; } = new Dictionary<string, ValueEntry>();

        public void SetValuesFromObject(object obj, string basePath = "")
        {
            if (obj == null)
                return;
            var objType = obj.GetType();
            if (objType.IsPrimitive || objType == typeof(string) || objType == typeof(DateTime))
            {
                SetValue(basePath, obj);
                return;
            }

            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var propValue = prop.GetValue(obj);
                var propPath = $"{basePath}.{prop.Name}";
                if (propValue == null)
                    continue;
                if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime))
                {
                    SetValue(propPath, propValue);
                }
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    var enumerable = propValue as System.Collections.IEnumerable;
                    int index = 0;
                    foreach (var item in enumerable)
                    {
                        var itemPath = $"{propPath}[{index}]";
                        SetValuesFromObject(item, itemPath);
                        index++;
                    }
                }
                else
                {
                    SetValuesFromObject(propValue, propPath);
                }
            }
        }

        public object ValueConversion(string value, FieldTypeEnum fieldType) {
        
            try
            {
                switch (fieldType)
                {
                    case FieldTypeEnum.String:
                        return value;
                    case FieldTypeEnum.Boolean:
                        if (bool.TryParse(value, out bool boolValue))
                            return boolValue;
                        break;
                    case FieldTypeEnum.Number:
                        if (double.TryParse(value, out double doubleValue))
                            return doubleValue;
                        break;
                    case FieldTypeEnum.DateTime:
                        if (DateTime.TryParse(value, out DateTime dateTimeValue))
                            return dateTimeValue;
                        break;
                    default:
                        return value;
                }
            }
            catch
            {
                // Handle or log conversion errors as needed
            }
            return value;
        }

        public void SetValuesFromConfiguration(IConfiguration config, FormElement root, string basePath = "")
        {
            foreach (var field in root.Fields)
            {
                var keyPath = $"{basePath}.{field.PropertyName}";
                var childEntries = config.GetSection(field.PropertyName).GetChildren();
                if (field.IsDictionary)
                {
                    var dictSection = config.GetSection(field.PropertyName);
                    int index = 0;
                    foreach (var child in dictSection.GetChildren())
                    {
                        var keyEntryPath = $"{keyPath}._Keys[{index}]";
                        var valueEntryPath = $"{keyPath}[{index}]";
                        SetValue(keyEntryPath, child.Key);
                        SetValuesFromConfiguration(child, field, valueEntryPath);
                        index++;
                    }
                }
                else if (field.IsArray)
                {
                    var arraySection = config.GetSection(field.PropertyName);
                    int index = 0;
                    foreach (var child in arraySection.GetChildren())
                    {
                        var itemPath = $"{keyPath}[{index}]";
                        if (field.Type == FieldTypeEnum.Object)
                        {
                            SetValuesFromConfiguration(child, field, itemPath);
                        }
                        else
                        {
                            SetValue(itemPath, ValueConversion(child.Value, field.Type));
                        }
                        index++;
                    }
                }
                else if (field.Type == FieldTypeEnum.Object)
                {
                    var objSection = config.GetSection(field.PropertyName);
                    SetValuesFromConfiguration(objSection, field, keyPath);
                }
                else
                {
                    var value = config[field.PropertyName];
                    
                    if(value !=null)
                        SetValue(keyPath, ValueConversion(value, field.Type));
                }
            }
        }
        
        public void AddDictionaryEntry(string propertyPath)
        {
            int index = 0;
            string fullKeyPath;
            string fullValuePath;
            do
            {
                fullKeyPath = $"{propertyPath}._Keys[{index}]";
                fullValuePath = $"{propertyPath}[{index}]";
                index++;
            } while (Values.ContainsKey(fullKeyPath) || Values.ContainsKey(fullValuePath));
            Values.Add(fullKeyPath, new ValueEntry());
            Values.Add(fullValuePath, new ValueEntry());
            OnValueChanged?.Invoke();
        }

        public void RemoveDictionaryEntry(string propertyPath, string keyPath)
        {
            if (Values.ContainsKey(keyPath))
            {
                Values.Remove(keyPath);
                var lastKeys = keyPath.LastIndexOf("._Keys");
                var valuePath = keyPath.Substring(0, lastKeys) + keyPath.Substring(lastKeys + 5);
                OnValueChanged?.Invoke();
            }
        }

        public bool PropertyExists(string propertyPath)
        {
            return Values.ContainsKey(propertyPath);
        }

        public T GetValue<T>(string propertyPath)
        {
            if (Values.ContainsKey(propertyPath))
                return (T)Values[propertyPath].Value;
            else
                return default(T);
        }

        public List<string> GetArrayKeysOfProperty(string propertyPath)
        {
            if(Values.Any(t=>t.Key.StartsWith(propertyPath+"._Keys")))
                return Values.Keys.Where(k => k.StartsWith(propertyPath + "._Keys[")).ToList();
            else
                return Values.Keys.Where(k => k.StartsWith(propertyPath + "[")).ToList();
        }

        public void RemoveValue(string propertyPath)
        {
            if (Values.ContainsKey(propertyPath))
            {
                Values.Remove(propertyPath);
                OnValueChanged?.Invoke();
            }
        }

        public void SetArrayElement<T>(string propertyPath, T values)
        {
            if (Values.ContainsKey(propertyPath))
                Values[propertyPath].UpdateValue(values);
            else
                Values.Add(propertyPath, new ValueEntry(values));
            OnValueChanged?.Invoke();
        }

        public string AddArrayElement(string propertyPath)
        {
            int index = 0;
            string fullPath;
            do
            {
                fullPath = $"{propertyPath}[{index}]";
                index++;
            } while (Values.ContainsKey(fullPath));
            Values.Add(fullPath, new ValueEntry());
            OnValueChanged?.Invoke();
            return fullPath;
        }

        public string AddArrayElement(string propertyPath, object value)
        {
            var fullPath = AddArrayElement(propertyPath);
            Values[fullPath].UpdateValue(value);
            OnValueChanged?.Invoke();
            return fullPath;
        }

        public List<Object> GetArrayValues(string propertyPath)
        {
            var keys = GetArrayKeysOfProperty(propertyPath);
            var list = new List<Object>();
            foreach (var key in keys)
            {
                var val = GetValue<object>(key);
                list.Add(val);
            }
            return list;
        }

        public string RemoveArrayElement(string propertyPath, object value)
        {
            var keys = GetArrayKeysOfProperty(propertyPath);
            foreach (var key in keys)
            {
                var val = GetValue<object>(key);
                if (val != null && val.Equals(value))
                {
                    Values.Remove(key);
                    OnValueChanged?.Invoke();
                    return key;
                }
            }
            return null;
        }

        public void SetValue<T>(string propertyPath, T value)
        {
            if (Values.ContainsKey(propertyPath))
                Values[propertyPath].UpdateValue(value);
            else
                Values.Add(propertyPath, new ValueEntry(value));

            OnValueChanged?.Invoke();
        }

        public JsonElement ToJson(FormRoot formRoot)
        {
            var dict = new Dictionary<string, object>();
            foreach (var field in formRoot.Fields)
            {
                AddFieldToDict(field, dict, "");
            }
            var json = JsonSerializer.Serialize(dict);
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                return doc.RootElement.Clone();
            }
        }

        public void AddFieldToDict(FormField field, Dictionary<string, object> dict, string propertyPath)
        {
            if (field.IsDictionary)
            {
                var subDict = new Dictionary<string, object>();
                var keys = GetArrayKeysOfProperty(propertyPath + "." + field.PropertyName + "._Keys");
                foreach (var key in keys)
                {
                    string keyValue = GetValue<string>(key);
                    if (!string.IsNullOrEmpty(keyValue))
                    {
                        string valueKey = key.Replace("._Keys", "");

                        var entryDict = new Dictionary<string, object>();
                        foreach (var subField in field.DictionaryValueFormFields)
                        {
                            AddFieldToDict(subField, entryDict, valueKey);
                        }

                        subDict[keyValue] = entryDict;
                    }
                }
                dict[field.PropertyName] = subDict;
            }
            else if (field.IsArray)
            {
                var list = new List<object>();
                var keys = GetArrayKeysOfProperty(propertyPath + "." + field.PropertyName);
                foreach (var key in keys)
                {
                    if (field.Type == FieldTypeEnum.Object)
                    {
                        var subDict = new Dictionary<string, object>();
                        foreach (var subField in field.Fields)
                        {
                            AddFieldToDict(subField, subDict, key);
                        }
                        list.Add(subDict);
                    }
                    else
                    {
                        var val = GetValue<object>(key);
                        list.Add(val);
                    }
                }
                dict[field.PropertyName] = list;
            }
            else if (field.Type == FieldTypeEnum.Object)
            {
                var subDict = new Dictionary<string, object>();
                foreach (var subField in field.Fields)
                {
                    AddFieldToDict(subField, subDict, propertyPath + "."+ field.PropertyName);
                }
                dict[field.PropertyName] = subDict;
            }
            else
            {
                var val = GetValue<object>(propertyPath + "." + field.PropertyName);
                dict[field.PropertyName] = val;
            }
        }
    }
}