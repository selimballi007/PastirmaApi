using PastirmaApi.Application.DTOs.AddressDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IAddressService
    {
        Task<AddressDTO> CreateAddressAsync(int userId, CreateAddressDTO dto);
        Task<AddressDTO> UpdateAddressAsync(int userId, int addressId, UpdateAddressDTO dto);
        Task<bool> DeleteAddressAsync(int userId, int addressId);
        Task<AddressDTO> GetAddressByIdAsync(int userId, int addressId);
        Task<List<AddressDTO>> GetUserAddressesAsync(int userId);
        Task<AddressDTO> SetDefaultAddressAsync(int userId, int addressId);
        Task<AddressDTO?> GetDefaultAddressAsync(int userId);
    }
}
