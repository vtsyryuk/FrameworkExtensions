namespace System
{
    public sealed class Ref<T>
    {
        private T _value;

        public Ref()
        {
        }

        public Ref(T value)
        {
            _value = value;
        }

        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    public static class Ref
    {
        public static Ref<T> Create<T>(T value)
        {
            return new Ref<T>(value);
        }
    }
}