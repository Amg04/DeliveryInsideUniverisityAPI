using DAL.Models;
using Microsoft.EntityFrameworkCore;
namespace BLLProject.Specifications
{
    // helper
    public static class SpecificationEvalutor<TEntity> where TEntity : ModelBase
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> input , ISpecification<TEntity> spec)
        {
            var query = input; 
            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria); 

            query = spec.Includes.Aggregate(query, (Current, IncludeExpression) => Current.Include(IncludeExpression));

            //// new 
            query = spec.ComplexIncludes.Aggregate(query, (current, includeQuery) => includeQuery(current));

            return query;
        }
    }

}
