using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.AddressDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data;

namespace PastirmaApi.Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AddressService> _logger;

        public AddressService(ApplicationDbContext context, ILogger<AddressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AddressDTO> CreateAddressAsync(int userId, CreateAddressDTO dto)
        {
            try
            {
                // If setting as default, unset other defaults
                if (dto.IsDefault)
                {
                    await UnsetOtherDefaultsAsync(userId);
                }

                var address = new Address
                {
                    UserId = userId,
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    AddressLine1 = dto.AddressLine1,
                    AddressLine2 = dto.AddressLine2,
                    City = dto.City,
                    District = dto.District,
                    PostalCode = dto.PostalCode,
                    Notes = dto.Notes,
                    IsDefault = dto.IsDefault
                };

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();

                return MapToDto(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for user {UserId}", userId);
                throw;
            }
        }

        public async Task<AddressDTO> UpdateAddressAsync(int userId, int addressId, UpdateAddressDTO dto)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

                if (address == null)
                {
                    throw new BusinessException("Adres bulunamadı.");
                }

                // If setting as default, unset other defaults
                if (dto.IsDefault && !address.IsDefault)
                {
                    await UnsetOtherDefaultsAsync(userId);
                }

                address.FullName = dto.FullName;
                address.Phone = dto.Phone;
                address.AddressLine1 = dto.AddressLine1;
                address.AddressLine2 = dto.AddressLine2;
                address.City = dto.City;
                address.District = dto.District;
                address.PostalCode = dto.PostalCode;
                address.Notes = dto.Notes;
                address.IsDefault = dto.IsDefault;

                await _context.SaveChangesAsync();

                return MapToDto(address);
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for user {UserId}", addressId, userId);
                throw;
            }
        }

        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

                if (address == null)
                {
                    throw new BusinessException("Adres bulunamadı.");
                }

                var wasDefault = address.IsDefault;

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();

                // If deleted address was default, set first remaining address as default
                if (wasDefault)
                {
                    var firstAddress = await _context.Addresses
                        .Where(a => a.UserId == userId)
                        .OrderBy(a => a.CreatedDate)
                        .FirstOrDefaultAsync();

                    if (firstAddress != null)
                    {
                        firstAddress.IsDefault = true;
                        await _context.SaveChangesAsync();
                    }
                }

                return true;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for user {UserId}", addressId, userId);
                throw;
            }
        }

        public async Task<AddressDTO> GetAddressByIdAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

                if (address == null)
                {
                    throw new BusinessException("Adres bulunamadı.");
                }

                return MapToDto(address);
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address {AddressId} for user {UserId}", addressId, userId);
                throw;
            }
        }

        public async Task<List<AddressDTO>> GetUserAddressesAsync(int userId)
        {
            try
            {
                var addresses = await _context.Addresses
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedDate)
                    .ToListAsync();

                return addresses.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for user {UserId}", userId);
                throw;
            }
        }

        public async Task<AddressDTO> SetDefaultAddressAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

                if (address == null)
                {
                    throw new BusinessException("Adres bulunamadı.");
                }

                // Unset other defaults
                await UnsetOtherDefaultsAsync(userId);

                // Set this address as default
                address.IsDefault = true;
                await _context.SaveChangesAsync();

                return MapToDto(address);
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for user {UserId}", addressId, userId);
                throw;
            }
        }

        public async Task<AddressDTO?> GetDefaultAddressAsync(int userId)
        {
            try
            {
                var address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

                return address == null ? null : MapToDto(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address for user {UserId}", userId);
                throw;
            }
        }

        private async Task UnsetOtherDefaultsAsync(int userId)
        {
            var defaultAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var addr in defaultAddresses)
            {
                addr.IsDefault = false;
            }

            if (defaultAddresses.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        private static AddressDTO MapToDto(Address address)
        {
            return new AddressDTO
            {
                Id = address.Id,
                FullName = address.FullName,
                Phone = address.Phone,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                District = address.District,
                PostalCode = address.PostalCode,
                Notes = address.Notes,
                UserId = address.UserId,
                IsDefault = address.IsDefault
            };
        }
    }
}
