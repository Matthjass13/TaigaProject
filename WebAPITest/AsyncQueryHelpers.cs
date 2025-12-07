using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace WebAPITest
{
    // Provider async pour EF Core
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression)
            => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(expression);

        // 🔴 C’est CETTE méthode qui corrige l’erreur :
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            => _inner.Execute<TResult>(expression);
    }


    // Enumerable async pour EF Core
    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    // Enumerator async
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
            => new ValueTask<bool>(_inner.MoveNext());
    }
}
