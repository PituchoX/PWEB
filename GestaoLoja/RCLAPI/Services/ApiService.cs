using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using RCLAPI.Models;

namespace RCLAPI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService? _storage;
        private string? _token;
        private UserInfoDto? _userInfo;

        private const string TOKEN_KEY = "mymedia_token";
        private const string USERINFO_KEY = "mymedia_userinfo";

        // URL base da API - configurada pelos projetos Web/MAUI
        public string ApiBaseUrl { get; set; } = "";

        // Evento para notificar mudanças de autenticação
        public event Action? OnAuthStateChanged;

        // Construtor sem storage (compatibilidade)
        public ApiService(HttpClient httpClient) : this(httpClient, null) { }

        // Construtor com storage
        public ApiService(HttpClient httpClient, ILocalStorageService? storage)
        {
            _httpClient = httpClient;
            _storage = storage;
        }

        // ==================== HELPERS ====================
        /// <summary>
        /// Obtém URL completa para imagens
        /// </summary>
        public string GetImageUrl(string? imagem)
        {
            if (string.IsNullOrEmpty(imagem))
                return "img/noproductstrans.png";
            
            return $"{ApiBaseUrl}img/{imagem}";
        }

        // ==================== PERSISTÊNCIA DA SESSÃO ====================
        /// <summary>
        /// Inicializa o serviço carregando a sessão guardada
        /// Deve ser chamado no OnInitializedAsync do MainLayout
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_storage == null) return;

            try
            {
                var token = await _storage.GetItemAsync(TOKEN_KEY);
                var userInfoJson = await _storage.GetItemAsync(USERINFO_KEY);

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userInfoJson))
                {
                    _token = token;
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                    
                    _userInfo = JsonSerializer.Deserialize<UserInfoDto>(userInfoJson);
                    OnAuthStateChanged?.Invoke();
                }
            }
            catch
            {
                // Se falhar a carregar, ignora (utilizador terá de fazer login)
            }
        }

        private async Task SaveSessionAsync()
        {
            if (_storage == null) return;

            try
            {
                if (!string.IsNullOrEmpty(_token) && _userInfo != null)
                {
                    await _storage.SetItemAsync(TOKEN_KEY, _token);
                    await _storage.SetItemAsync(USERINFO_KEY, JsonSerializer.Serialize(_userInfo));
                }
            }
            catch
            {
                // Ignora erros de armazenamento
            }
        }

        private async Task ClearSessionAsync()
        {
            if (_storage == null) return;

            try
            {
                await _storage.RemoveItemAsync(TOKEN_KEY);
                await _storage.RemoveItemAsync(USERINFO_KEY);
            }
            catch
            {
                // Ignora erros
            }
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
            OnAuthStateChanged?.Invoke();
        }

        public string? GetToken() => _token;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
        public UserInfoDto? GetUserInfo() => _userInfo;

        public void SetUserInfo(UserInfoDto? userInfo)
        {
            _userInfo = userInfo;
            OnAuthStateChanged?.Invoke();
        }

        public async Task LogoutAsync()
        {
            SetToken(null);
            _userInfo = null;
            await ClearSessionAsync();
            OnAuthStateChanged?.Invoke();
        }

        // Método síncrono para compatibilidade
        public void Logout()
        {
            SetToken(null);
            _userInfo = null;
            _ = ClearSessionAsync(); // Fire and forget
            OnAuthStateChanged?.Invoke();
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
                        
                        // Guardar sessão localmente
                        await SaveSessionAsync();
                        
                        OnAuthStateChanged?.Invoke();
                    }
                    return result ?? new AuthResponseDto { Success = false, Message = "Erro ao processar resposta" };
                }
                else
                {
                    // Tentar ler a mensagem de erro da API
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                        if (errorResult != null && !string.IsNullOrEmpty(errorResult.Message))
                        {
                            return errorResult;
                        }
                    }
                    catch
                    {
                        // Se não conseguir ler como JSON, tenta como texto
                        var errorText = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(errorText))
                        {
                            return new AuthResponseDto { Success = false, Message = errorText };
                        }
                    }
                    
                    return new AuthResponseDto { Success = false, Message = "Credenciais inválidas ou conta não ativa." };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Success = false, Message = $"Erro de ligação: {ex.Message}" };
            }
        }

        public async Task<AuthResponseDto> RegisterClienteAsync(RegisterClienteDto register)
        {
            try
            {
                // Mapear para o formato que a API espera (Nome em vez de NomeCompleto)
                var payload = new
                {
                    Nome = register.NomeCompleto,
                    Email = register.Email,
                    NIF = register.NIF ?? "999999990",
                    Password = register.Password
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/register-cliente", payload);

                // 1. Verificar se a API respondeu com Sucesso (200 OK)
                if (response.IsSuccessStatusCode)
                {
                    // Se tiver conteúdo JSON, lê
                    if (response.Content.Headers.ContentLength > 0)
                    {
                        try
                        {
                            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                            return result ?? new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso! Aguarde aprovação por um administrador." };
                        }
                        catch
                        {
                            // Se falhar a ler o JSON mas for 200 OK, assumimos sucesso
                            return new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso! Aguarde aprovação por um administrador." };
                        }
                    }

                    // Se for 200 OK mas corpo vazio
                    return new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso! Aguarde aprovação por um administrador." };
                }
                else
                {
                    // 2. Se deu erro (ex: 400 ou 500), lê a mensagem como TEXTO simples
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
                // Mapear para o formato que a API espera (Nome em vez de NomeCompleto)
                var payload = new
                {
                    Nome = register.NomeCompleto,
                    NomeEmpresa = register.NomeEmpresa,
                    Email = register.Email,
                    NIF = register.NIF ?? "999999990",
                    Password = register.Password
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/register-fornecedor", payload);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content.Headers.ContentLength > 0)
                    {
                        try
                        {
                            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                            return result ?? new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso! Aguarde aprovação." };
                        }
                        catch
                        {
                            return new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso! Aguarde aprovação." };
                        }
                    }
                    return new AuthResponseDto { Success = true, Message = "Registo efetuado com sucesso! Aguarde aprovação." };
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

        public async Task<List<ProdutoDto>> PesquisarProdutosAsync(string termo)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<ProdutoDto>>($"api/produtos/pesquisa?q={Uri.EscapeDataString(termo)}");
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
        public async Task<(VendaDto? Venda, string? Erro)> CriarVendaAsync(VendaCreateDto venda)
        {
            try
            {
                // Converter para o formato que a API espera (List<ItemVendaDto>)
                var itens = venda.Linhas.Select(l => new { ProdutoId = l.ProdutoId, Quantidade = l.Quantidade }).ToList();
                
                var response = await _httpClient.PostAsJsonAsync("api/vendas", itens);
                
                if (response.IsSuccessStatusCode)
                {
                    // A API devolve { VendaId, Total, Estado, Message }
                    var result = await response.Content.ReadFromJsonAsync<VendaCriadaResponseDto>();
                    if (result != null)
                    {
                        return (new VendaDto
                        {
                            Id = result.VendaId,
                            Estado = result.Estado ?? "Pendente",
                            Total = result.Total
                        }, null);
                    }
                }
                else
                {
                    // Ler mensagem de erro - tentar como JSON primeiro
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    // Tentar extrair a mensagem do JSON
                    try
                    {
                        var errorObj = JsonDocument.Parse(errorContent);
                        if (errorObj.RootElement.TryGetProperty("Message", out var msgProp) ||
                            errorObj.RootElement.TryGetProperty("message", out msgProp))
                        {
                            return (null, msgProp.GetString());
                        }
                    }
                    catch
                    {
                        // Se não for JSON válido, usa o texto diretamente
                    }
                    
                    // Verificar códigos de erro específicos
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return (null, "Sessão expirada. Por favor faça login novamente.");
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return (null, "Não tem permissão para realizar esta operação. Apenas Clientes podem fazer compras.");
                    }
                    
                    return (null, string.IsNullOrEmpty(errorContent) ? $"Erro {(int)response.StatusCode}" : errorContent);
                }
                return (null, "Erro desconhecido ao criar venda.");
            }
            catch (Exception ex)
            {
                return (null, $"Erro de ligação: {ex.Message}");
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

        public async Task<bool> SimularPagamentoAsync(int vendaId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/vendas/{vendaId}/pagar", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
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

        public async Task<List<VendaFornecedorDto>> GetMinhasVendasFornecedorAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<VendaFornecedorDto>>("api/fornecedor/produtos/vendas");
                return result ?? new List<VendaFornecedorDto>();
            }
            catch
            {
                return new List<VendaFornecedorDto>();
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

        /// <summary>
        /// Suspende um produto (retira da listagem/venda)
        /// </summary>
        public async Task<bool> SuspenderProdutoAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/fornecedor/produtos/{id}/suspender", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reativa um produto suspenso (volta para Pendente aguardando aprovação)
        /// </summary>
        public async Task<bool> ReativarProdutoAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/fornecedor/produtos/{id}/reativar", null);
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

        // ==================== UPLOAD DE IMAGENS ====================
        /// <summary>
        /// Faz upload de uma imagem de produto para a API
        /// </summary>
        /// <param name="fileContent">Conteúdo do ficheiro em bytes</param>
        /// <param name="fileName">Nome original do ficheiro</param>
        /// <param name="contentType">Tipo MIME do ficheiro</param>
        /// <returns>Nome do ficheiro guardado ou null se falhar</returns>
        public async Task<string?> UploadImagemProdutoAsync(byte[] fileContent, string fileName, string contentType)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var fileContentData = new ByteArrayContent(fileContent);
                fileContentData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                content.Add(fileContentData, "ficheiro", fileName);

                var response = await _httpClient.PostAsync("api/imagens/produto", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ImagemUploadResponseDto>();
                    return result?.NomeFicheiro;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// DTO para resposta de upload de imagem
    /// </summary>
    public class ImagemUploadResponseDto
    {
        public bool Success { get; set; }
        public string? NomeFicheiro { get; set; }
        public string? Message { get; set; }
    }
}
