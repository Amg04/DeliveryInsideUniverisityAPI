﻿using DAL.Models;
using System.Linq.Expressions;
namespace BLLProject.Specifications
{
    public class BaseSpecification<T> : ISpecification<T> where T : ModelBase
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set ; } = new
            List<Expression<Func<T, object>>>();

        public List<Func<IQueryable<T>, IQueryable<T>>> ComplexIncludes { get; set; } = new();

        // with no Include
        public BaseSpecification()
        {

        }
        public BaseSpecification(Expression<Func<T, bool>> CriteriaExpression )
        {
            Criteria = CriteriaExpression;
        }

        // new 
        // Constructor مع IncludeQueryable
        public BaseSpecification(Expression<Func<T, bool>> criteriaExpression, Func<IQueryable<T>, IQueryable<T>> includeQuery)
        {
            Criteria = criteriaExpression;
            ComplexIncludes.Add(includeQuery);
        }

    }

}
