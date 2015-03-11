namespace System
{
    public static class DisposableExtensions
    {
        private sealed class SafeDisposable : IDisposable
        {
            private readonly IDisposable _disposable;

            public SafeDisposable(IDisposable disposable)
            {
                _disposable = disposable;
            }

            void IDisposable.Dispose()
            {
                try
                {
                    _disposable.Dispose();
                }
                catch (Exception)
                {
                }
            }
        }

        public static IDisposable CreateSafeDisposer(this IDisposable disposable)
        {
            if (disposable == null) throw new ArgumentNullException("disposable");

            return new SafeDisposable(disposable);
        }
    }
}