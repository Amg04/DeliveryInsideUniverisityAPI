using DAL.Models;
using System.Linq.Expressions;

namespace BLLProject.Specifications
{
    // related by => linq spec (operator) => Include | Where
    public interface ISpecification<T> where T : ModelBase
    {
        public Expression<Func<T, bool>> Criteria { get; }
        public List<Expression<Func<T, object >>> Includes { get; }
        // new to  allow thenInclude
        List<Func<IQueryable<T>, IQueryable<T>>> ComplexIncludes { get; }
    }

}
