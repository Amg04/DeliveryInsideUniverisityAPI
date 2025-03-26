using DAL.Models;
using System.Linq.Expressions;

namespace BLLProject.Specifications
{
    // related by => linq spec (operator) => Include | Where
    public interface ISpecification<T> where T : class, IModelBase
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object >>> Includes { get; set; }
        // new 
        List<Func<IQueryable<T>, IQueryable<T>>> IncludeQueries { get; set; }
    }

}
