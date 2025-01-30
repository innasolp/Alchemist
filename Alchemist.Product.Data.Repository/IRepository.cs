using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alchemist.Product.Data.Repository;

public interface IRepository<T,TId>
    where T : class
    where TId : struct
{
    TId Add(T entity);

    bool Delete(TId id);

    bool Update(T entity);

    TId[] Add(T[] entities);

    bool Delete(TId[] ids);

    bool Update(T[] entities);

    T Get(TId id);

    T[] GetAll();
}
