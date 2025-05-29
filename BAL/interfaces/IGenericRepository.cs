using BLLProject.Specifications;
using DAL.Models;

namespace BLLProject.interfaces
{
    public interface IGenericRepository<T> where T : ModelBase
    {
        public void Add(T entity);
        public void Update(T entity); // All
        public void Delete(T entity);
        public T Get(int Id);
        public IEnumerable<T> GetAll();

        public T GetEntityWithSpec(ISpecification<T> spec);
        public IEnumerable<T> GetAllWithSpec(ISpecification<T> spec);
    }
}
