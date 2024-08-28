using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

public partial class JsonData
{
    [ContextProperty("ИндексыМассива", "ArrayIndexes")]
    public JsonData_ForEachAdapterArrayIndexes ForEachAdapterArrayIndexes => new(this);

    [ContextProperty("ЗначенияМассива", "ArrayValues")]
    public JsonData_ForEachAdapterArrayValues ForEachAdapterArrayValues => new(this);
    
    [ContextProperty("КлючиОбъекта", "ObjectKeys")]
    public JsonData_ForEachAdapterObjectKeys ForEachAdapterObjectKeys => new(this);

    [ContextProperty("ЗначенияОбъекта", "ObjectValues")]
    public JsonData_ForEachAdapterObjectValues ForEachAdapterObjectValues => new(this);

    public class JsonData_ForEachAdapterArrayIndexes :
        AutoContext<JsonData_ForEachAdapterArrayIndexes>,
        ICollectionContext,
        IEnumerable<IValue>
    {
        private JsonData _jsonData;
        private JArray _array;

        public JsonData_ForEachAdapterArrayIndexes(JsonData jsonData)
        {
            if (jsonData.Root is not JArray array)
                throw new RuntimeException(
                    "Виртуальный объект 'ИндексыМассива' невозможно создать: текущий JsonData не представляет из себя массив.");

            _jsonData = jsonData;
            _array = array;
        }

        public int Count()
        {
            return _array.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(this.GetEnumerator());
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            for (int i = 0; i < _array.Count; i++)
                yield return ValueFactory.Create(i);
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
    }

    public class JsonData_ForEachAdapterArrayValues :
        AutoContext<JsonData_ForEachAdapterArrayValues>,
        ICollectionContext,
        IEnumerable<IValue>
    {
        private JsonData _jsonData;
        private JArray _array;

        public JsonData_ForEachAdapterArrayValues(JsonData jsonData)
        {
            if (jsonData.Root is not JArray array)
                throw new RuntimeException(
                    "Виртуальный объект 'ЗначенияМассива' невозможно создать: текущий JsonData не представляет из себя массив."
                );

            _jsonData = jsonData;
            _array = array;
        }

        public int Count()
        {
            return _array.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(this.GetEnumerator());
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            return _array
                .Select(value => IntoOneScriptType(value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
    }
    
        public class JsonData_ForEachAdapterObjectKeys :
        AutoContext<JsonData_ForEachAdapterObjectKeys>,
        ICollectionContext,
        IEnumerable<IValue>
    {
        private JsonData _jsonData;
        private JObject _object;

        public JsonData_ForEachAdapterObjectKeys(JsonData jsonData)
        {
            if (jsonData.Root is not JObject jObject)
                throw new RuntimeException(
                    "Виртуальный объект 'КлючиОбъекта' невозможно создать: текущий JsonData не представляет из себя объект.");

            _jsonData = jsonData;
            _object = jObject;
        }

        public int Count()
        {
            return _object.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(this.GetEnumerator());
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            return _object
                .Properties()
                .Select(property => ValueFactory.Create(property.Name))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
    }

    public class JsonData_ForEachAdapterObjectValues :
        AutoContext<JsonData_ForEachAdapterObjectValues>,
        ICollectionContext,
        IEnumerable<IValue>
    {
        private JsonData _jsonData;
        private JObject _object;

        public JsonData_ForEachAdapterObjectValues(JsonData jsonData)
        {
            if (jsonData.Root is not JObject jObject)
                throw new RuntimeException(
                    "Виртуальный объект 'ЗначенияОбъекта' невозможно создать: текущий JsonData не представляет из себя объект."
                );

            _jsonData = jsonData;
            _object = jObject;
        }

        public int Count()
        {
            return _object.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(this.GetEnumerator());
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            return _object
                .Properties()
                .Select(value => IntoOneScriptType(value.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
    }
}