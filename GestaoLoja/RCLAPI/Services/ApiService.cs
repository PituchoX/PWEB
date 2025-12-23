using System.Net.Http.Json;
using System.Net.Http.Headers;
using RCLAPI.Models;

namespace RCLAPI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string? _token;
        private UserInfoDto? _userInfo;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ==================== TOKEN/AUTH ====================
        public void SetToken(string? token)
        {
            _token = token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public string? GetToken() => _token;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
        public UserInfoDto? GetUserInfo() => _userInfo;

        public void SetUserInfo(UserInfoDto? userInfo)
        {
            _userInfo = userInfo;
        }

        public void Logout()
        {
            SetToken(null);
            _userInfo = null;
        }

        // ==================== AUTENTICAÇÃO ====================
        public async Task<AuthResponseDto> LoginAsync(LoginDto login)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", login);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                    if (result != null && result.Success && !string.IsNullOrEmpty(result.Token))
                    {
                        SetToken(result.Token);
                        _userInfo = new UserInfoDto
                        {
                            UserId = result.UserId ?? "",
                            Email = result.Email ?? "",
                            NomeCompleto = result.NomeCompleto ?? "",
                            Perfil = result.Perfil ?? ""
                        };
                    }
                    return result ?? new AuthResponseDto { Success = false, Message = "Erro ao processar resposta" };
                }
                return new AuthResponseDto { Success = false, Message = "Credenciais inválidas" };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Success = false, Message = $"Erro: {ex.Message}" };
            }
        }

        // ... (dentro da classe ApiService)

        public async Task<AuthResponseDto> RegisterClienteAsync(RegisterClienteDto register)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register/cliente", register);

                // 1. Verificar se a API respondeu com Sucesso (200 OK)
                if (response.IsSuccessStatusCode)
                {
                    // Se tiver conteúdo JSON, lê
                    if (response.Content.Headers.ContentLength > 0)
                    {
                        try
                        {
                            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                            return result ?? new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso." };
                        }
                        catch
                        {
                            // Se falhar a ler o JSON mas for 200 OK, assumimos sucesso
                            return new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso." };
                        }
                    }

                    // Se for 200 OK mas corpo vazio
                    return new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso." };
                }
                else
                {
                    // 2. Se deu erro (ex: 400 ou 500), lê a mensagem como TEXTO simples
                    // Isto evita o erro "The input does not contain any JSON tokens"
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = string.IsNullOrEmpty(errorMessage) ? "Erro desconhecido ao registar." : errorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Success = false, Message = $"Erro de ligação: {ex.Message}" };
            }
        }

        public async Task<AuthResponseDto> RegisterFornecedorAsync(RegisterFornecedorDto register)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register/fornecedor", register);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content.Headers.ContentLength > 0)
                    {
                        try
                        {
                            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                            return result ?? new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso." };
                        }
                        catch
                        {
                            return new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso." };
                        }
                    }
                    return new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso." };
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = string.IsNullOrEmpty(errorMessage) ? "Erro desconhecido ao registar." : errorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Success = false, Message = $"Erro de ligação: {ex.Message}" };
            }
        }

        // ==================== CATEGORIAS ====================
        public async Task<List<CategoriaDto>> GetCategoriasAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<CategoriaDto>>("api/categorias");
                return result ?? new List<CategoriaDto>();
            }
            catch
            {
                return new List<CategoriaDto>();
            }
        }

        public async Task<CategoriaDto?> GetCategoriaAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CategoriaDto>($"api/categorias/{id}");
            }
            catch
            {
                return null;
            }
        }

        // ==================== PRODUTOS ====================
        public async Task<List<ProdutoDto>> GetProdutosAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ProdutoDto>>("api/produtos");
                return result ?? new List<ProdutoDto>();
            }
            catch
            {
                return new List<ProdutoDto>();
            }
        }

        public async Task<List<ProdutoDto>> GetProdutosPorCategoriaAsync(int categoriaId)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ProdutoDto>>($"api/produtos/categoria/{categoriaId}");
                return result ?? new List<ProdutoDto>();
            }
            catch
            {
                return new List<ProdutoDto>();
            }
        }

        public async Task<ProdutoDto?> GetProdutoDestaqueAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProdutoDto>("api/produtos/destaque");
            }
            catch
            {
                return null;
            }
        }

        public async Task<ProdutoDto?> GetProdutoAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProdutoDto>($"api/produtos/{id}");
            }
            catch
            {
                return null;
            }
        }

        // ==================== MODOS DE ENTREGA ====================
        public async Task<List<ModoEntregaDto>> GetModosEntregaAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ModoEntregaDto>>("api/modosentrega");
                return result ?? new List<ModoEntregaDto>();
            }
            catch
            {
                return new List<ModoEntregaDto>();
            }
        }

        // ==================== VENDAS (CLIENTE) ====================
        public async Task<VendaDto?> CriarVendaAsync(VendaCreateDto venda)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/vendas", venda);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<VendaDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<VendaDto>> GetMinhasVendasAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<VendaDto>>("api/vendas/minhas");
                return result ?? new List<VendaDto>();
            }
            catch
            {
                return new List<VendaDto>();
            }
        }

        // ==================== FORNECEDOR - MEUS PRODUTOS ====================
        public async Task<List<ProdutoDto>> GetMeusProdutosAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ProdutoDto>>("api/fornecedor/produtos");
                return result ?? new List<ProdutoDto>();
            }
            catch
            {
                return new List<ProdutoDto>();
            }
        }

        public async Task<bool> CriarProdutoAsync(ProdutoCreateDto produto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/fornecedor/produtos", produto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AtualizarProdutoAsync(ProdutoUpdateDto produto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/fornecedor/produtos/{produto.Id}", produto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApagarProdutoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/fornecedor/produtos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
