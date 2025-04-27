using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BLLProject.interfaces;
using BLLProject.Specifications;
using DAL.Models;
using DAL.Data;


namespace BLLProject.Repositories
{

    public class GenericRepository<T> : IGenericRepository<T> where T : ModelBase
    {
        protected readonly RestaurantAPIContext dbContext;
        public GenericRepository(RestaurantAPIContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Add( T entity)
        {
            dbContext.Set<T>().Add(entity);
        }

        public void Update(T entity)
        {
            dbContext.Set<T>().Update(entity);
        }

        public  void Delete(T entity)
        {
            dbContext.Set<T>().Remove(entity);
        }

        public T Get(int Id) => dbContext.Set<T>().Find(Id);
        public IEnumerable<T> GetAll() => dbContext.Set<T>().AsNoTracking().ToList();

        public T GetEntityWithSpec(ISpecification<T> spec) =>
           ApplySpecification(spec).FirstOrDefault();
        public IEnumerable<T> GetAllWithSpec(ISpecification<T> spec) =>
            ApplySpecification(spec).AsNoTracking().ToList();

        private IQueryable<T> ApplySpecification(ISpecification<T> spec)=>
            SpecificationEvalutor<T>.GetQuery(dbContext.Set<T>(), spec);
    }
}
