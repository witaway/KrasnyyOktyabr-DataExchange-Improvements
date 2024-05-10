﻿using KrasnyyOktyabr.JsonTransform.Numerics;

namespace KrasnyyOktyabr.JsonTransform.Expressions;

/// <exception cref="ArgumentException"></exception>
public sealed class RoundExpression : AbstractExpression<Task<Number>>
{
    private readonly IExpression<Task<Number>> _valueExpression;

    private readonly IExpression<Task<int>>? _digitsExpression;

    public RoundExpression(IExpression<Task<Number>> valueExpression, IExpression<Task<int>>? digitsExpression = null)
    {
        _valueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));

        if (digitsExpression != null)
        {
            _digitsExpression = digitsExpression;
        }
    }

    public override async Task<Number> InterpretAsync(IContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            int digits = 0;

            if (_digitsExpression != null)
            {
                digits = await _digitsExpression.InterpretAsync(context, cancellationToken).ConfigureAwait(false);
            }

            if (digits < 0)
            {
                throw new ArgumentException($"Negative digits ({digits}) not allowed");
            }

            Number value = await _valueExpression.InterpretAsync(context, cancellationToken).ConfigureAwait(false);

            if (value.Int != null)
            {
                return value;
            }

            if (value.Double != null)
            {
                return new Number(Math.Round(value.Double.Value, digits, MidpointRounding.AwayFromZero));
            }

            throw new NotImplementedException();
        }
        catch (InterpretException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InterpretException(ex.Message, Mark);
        }
    }
}
