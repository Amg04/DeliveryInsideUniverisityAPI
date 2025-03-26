using BLLProject.Specifications;
using DAL.Models;

namespace BLLProject.interfaces
{
    public interface IGenaricRepository<T> where T : class,IModelBase
    {
        public void Add(T entity);
        public void Update(T entity); // All
        public void Delete(T entity);
        public T Get(int Id);
        public IEnumerable<T> GetAll();

        public T GetEntityWithSpec(ISpecification<T> spec); // where
        public IEnumerable<T> GetAllWithSpec(ISpecification<T> spec); // include
    }
}
