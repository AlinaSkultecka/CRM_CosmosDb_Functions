using CrmApi.Data.Entities;

namespace CrmApi.Data.Interface
{
    public interface ICustomerRepo
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(string id);
        Task<Customer> AddAsync(Customer customer);
        Task<Customer?> UpdateAsync(string id, Customer updatedCustomer);
        Task<bool> DeleteAsync(string id);
        Task<List<Customer>> SearchByCustomerNameAsync(string name);
        Task<List<Customer>> SearchBySellerNameAsync(string sellerName);
    }
}
