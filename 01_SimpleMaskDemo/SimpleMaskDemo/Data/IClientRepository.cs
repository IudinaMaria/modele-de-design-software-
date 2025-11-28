using System.Collections.Generic;
using SimpleMaskDemo.Domain;

namespace SimpleMaskDemo.Data;

// описыает что может интерейс
public interface IClientRepository
{
    void Add(Client c);
    IEnumerable<Client> GetAll();

    IEnumerable<Client> FindByName(string firstName);

    // для битовой source откуда берем, compareMask фильтр,  copyMask копирует
    int CopyDataByMask(Client source, ClientFields compareMask, ClientFields copyMask);
}
