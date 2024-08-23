﻿using System;
using ScriptEngine.Machine.Contexts;

namespace KrasnyyOktyabr.Scripting.OneScript.Logic.Api;

[ContextClass("СервисПогоды", "WeatherService")]
public class WeatherProvider : AutoContext<WeatherProvider>
{
    private readonly Random random = new Random();

    [ContextMethod("ПолучитьПогоду", "GetWeather")]
    public WeatherData GetWeather()
    {
        return new WeatherData
        {
            Temperature = GetRandomNumber(-20, 45),
            Humidity = GetRandomNumber(30, 100),
            Pressure = GetRandomNumber(740, 800)
        };
    }

    private decimal GetRandomNumber(int min, int max)
    {
        return (decimal)(random.Next(min, max) * random.NextDouble());
    }
}