using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Application.Mappers
{
    public static class AddressMapper
    {
        public static AddressDto ToDto(this Address address)
        {
            if (address == null) return null!;

            return new AddressDto(
                ZipCode: address.ZipCode,
                Number: address.Number,
                Neighborhood: string.IsNullOrWhiteSpace(address.Neighborhood) ? null : address.Neighborhood,
                Street: string.IsNullOrWhiteSpace(address.Street) ? null : address.Street,
                City: string.IsNullOrWhiteSpace(address.City) ? null : address.City,
                State: string.IsNullOrWhiteSpace(address.State) ? null : address.State
            );
        }

        public static Address ToEntity(this AddressDto dto)
        {
            if (dto == null) return null!;

            return new Address
            {
                ZipCode = dto.ZipCode,
                Number = dto.Number,
                Neighborhood = dto.Neighborhood ?? string.Empty,
                Street = dto.Street ?? string.Empty,
                City = dto.City ?? string.Empty,
                State = dto.State ?? string.Empty
            };
        }
    }

}
