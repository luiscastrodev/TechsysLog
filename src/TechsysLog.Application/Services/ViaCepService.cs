using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TechsysLog.Application.DTOS;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Entities;

namespace TechsysLog.Application.Services
{
    public class ViaCepService : ICepService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Address?> GetAddressByCepAsync(string zipcode)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ViaCepResponseDto>($"https://viacep.com.br/ws/{zipcode}/json/");

                if (response == null || response.erro) return null;

                return new Address
                {
                    ZipCode = response.cep ?? zipcode,
                    Street = response.logradouro ?? "",
                    Neighborhood = response.bairro ?? "",
                    City = response.localidade ?? "",
                    State = response.uf ?? ""
                };
            }
            catch(Exception ex)
            {
                _logger.LogError("Erro ao obter dados do VIACEP {ex}", ex);

                return null;
            }
        }
    }
}
