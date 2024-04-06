﻿using KrasnyyOktyabr.JsonTransform.Numerics;

namespace KrasnyyOktyabr.JsonTransform.Expressions;

/// <summary>
/// Divides 2 numbers.
/// </summary>
/// <exception cref="ArgumentNullException"></exception>
public sealed class DivideExpression(IExpression<Task<Number>> leftExpression, IExpression<Task<Number>> rightExpression)
    : AbstractBinaryExpression<Number>(leftExpression, rightExpression)
{
    /// <exception cref="DivideByZeroException"></exception>
    protected override ValueTask<Number> CalculateAsync(Number left, Number right)
    {
        return ValueTask.FromResult(left / right);
    }
}
