public record struct CPI(double Value);
public record struct Energy(double Value);

public record struct IntLog2(double Value);

public class IntPowerOf2 {
    private int Value;
    private IntPowerOf2? Min, Max;

    private void _constructor(int value, IntPowerOf2? min, IntPowerOf2? max) {
        Value = value;
        Min = min;
        Max = max;
        Validate();
    }
    public IntPowerOf2(int value, IntPowerOf2? min = null, IntPowerOf2? max = null) {
        _constructor(value, min, max);
    }

    public IntPowerOf2(IntLog2 value, IntPowerOf2? min = null, IntPowerOf2? max = null) {
        _constructor((int)Math.Pow(2, value.Value), min, max);
    }

    private void Validate() {
        if (Value <= 0 || (Value & (Value - 1)) != 0)
        {
            throw new ArgumentException($"The given IntPowerOf2 must be a positive power of 2. Received value: {Value}.", nameof(Value));
        }
        if (Min is not null && Value < Min.GetValueInt())
        {
            throw new ArgumentException($"The given IntPowerOf2 must be bigger or equal than {Min.GetValueInt()}. Received value: {Value}.", nameof(Value));
        }
        if (Max is not null && Value > Max.GetValueInt())
        {
            throw new ArgumentException($"The given IntPowerOf2 must be greater or equal than {Max.GetValueInt()}. Received value: {Value}.", nameof(Value));
        }
    }

    public int GetValueInt() {
        return Value;
    }

    public IntLog2 GetValueLog2() {
        return new(Math.Log2(Value));
    }

    public void SetValue(int value) {
        Value = value;
        Validate();
    }

    public void SetValue(IntLog2 value) {
        SetValue((int)Math.Pow(2, value.Value));
    }

    public override string ToString()
    {
        return $"{Value}";
    }
}