using System;
using System.IO;
using System.Text;
using KrasnyyOktyabr.Scripting.OneScript.Logic.Api;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.ScriptedWorker
{
    [ContextClass("Обработчик", "Worker")]
    public class Worker : AutoScriptDrivenObject<Worker>
    {
        public static string ConsumerInstructionsPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Properties", "ConsumerInstructions");
        
        private Worker(LoadedModule module) : base(module)
        {
        }

        /// <summary>
        /// Вызывает в скрипте метод ОбработатьПорциюДанных/ProcessDataPortion
        /// Метод должен иметь сигнатуру Функция ОбработатьПорциюДанных(СервисПогоды, Отказ)
        /// и возвращать булево
        /// </summary>
        /// <returns>Если true то обработка успешна, если false, то нет</returns>
        /// <exception cref="RuntimeException">Если произошла ошибка выполнения скрипта</exception>
        /// <exception cref="OperationCanceledException">Выбрасывает исключение, если в обработчике выставили Отказ = Истина</exception>
        public JsonData ProccessScript(JsonData inputJsonData)
        {
            var handlerId = GetScriptMethod("ОбработатьПорциюДанных", "ProcessDataPortion");
            if (handlerId == -1)
            {
                throw new RuntimeException("Не найден обработчик \"ОбработатьПорциюДанных\"(\"ProcessDataPortion\")");
            }

            var methodInfo = Module.Methods[handlerId];
            if (!methodInfo.Signature.IsFunction || methodInfo.Signature.Params.Length != 2)
            {
                throw new RuntimeException("Обработчик должен быть функцией с двумя параметрами");
            }

            if (methodInfo.Signature.Params[1].IsByValue)
            {
                throw new RuntimeException("Параметр Отказ не должен иметь признак Знач");
            }
            
            // Передаем в метод 2 параметра. Второй - выходной параметр "Отказ"
            //
            // Первый параметр - входные "Данные"
            // Второй параметр - "Отказ"
            //
            // Переменные Данные и Отказ должны быть типом "Переменная" и передаваться по ссылке, чтобы
            // в них можно было записать значение в скрипте и получить на уровне C#
            var jsonDataArg = Variable.Create(ValueFactory.Create(inputJsonData), "Данные");
            var cancelArg = Variable.Create(ValueFactory.Create(""), "Отказ");

            var returned = CallScriptMethod(handlerId, new[]
            {
                jsonDataArg,
                cancelArg
            });

            // Проверка на Отказ. Если в переменной непустая строка - ее установили в скрипте через Отказ = "Причина отказа"
            var cancelArgStr = cancelArg.Value.ToString();
                
            if (cancelArgStr.Length > 0)
            {
                throw new OperationCanceledException(cancelArgStr);
            }
            
            // Проверяем возвращённое значение
            if (returned is not JsonData returnedJsonData)
            {
                throw new RuntimeException("Обработчик должен вернуть объект JsonData");
            }

            return returnedJsonData;
        }

        /// <summary>
        /// Создать из файлового скрипта
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="scriptFileName"></param>
        /// <returns></returns>
        public static Worker CreateFromFile(ScriptingEngine engine, string scriptFileName)
        {
            string scriptFilePath = Path.Combine(ConsumerInstructionsPath, scriptFileName);
            var code = engine.Loader.FromFile(scriptFilePath);
            return Create(engine, code);
        }

        /// <summary>
        /// Создать из строки текста
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="scriptFileName"></param>
        /// <returns></returns>
        public static Worker CreateFromString(ScriptingEngine engine, string scriptContent)
        {
            var code = engine.Loader.FromString(scriptContent);
            return Create(engine, code);
        }

        /// <summary>
        /// Создать из текстового потока
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="scriptFileName"></param>
        /// <returns></returns>
        public static Worker CreateFromScriptStream(ScriptingEngine engine, Stream stream)
        {
            using var streamReader = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
            var scriptContent = streamReader.ReadToEnd();
            return CreateFromString(engine, scriptContent);
        }

        /// <summary>
        /// Создать из скрипта, который взялся откуда-то еще
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Worker Create(ScriptingEngine engine, ICodeSource code)
        {
            var compiler = engine.GetCompilerService();

            // Можно добавить символы, видимые компилятору в рамках именно этого модуля
            // Наследование от AutoScriptDrivenObject автоматически добавляет свойства и методы класса
            //compiler.DefineMethod();
            //compiler.DefineVariable();
            //compiler.DefinePreprocessorValue();

            var module = CompileModule(compiler, code);
            var loadedModule = engine.LoadModuleImage(module);

            return new Worker(loadedModule);
        }
    }
}