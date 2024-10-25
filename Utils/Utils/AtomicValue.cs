namespace Tajlo4ekUtils
{
    public class AtomicValue<T>
    {
        private readonly object _lock = new object();

        private T _value;

        public AtomicValue(T data)
        {
            _value = data;
        }

        public AtomicValue() : this(default)
        {
        }

        public T Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }
            }
            set
            {
                lock (_lock)
                {
                    _value = value;
                }
            }
        }

    }
}
