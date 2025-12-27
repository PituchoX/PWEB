using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Dtos;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace RESTfulAPIPWeb.Controllers
{
    /// <summary>
    /// Controller para vendas/compras da API MyMEDIA
    /// Apenas endpoints para Clientes - Admin/Funcionário gerem vendas na GestaoLoja
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class VendasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VendasController> _logger;

        public VendasController(AppDbContext context, ILogger<VendasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Cliente cria uma nova venda/encomenda
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> CriarVenda([FromBody] List<ItemVendaDto> itens)
        {
            _logger.LogInformation("=== CRIAR VENDA ===");
            
            if (itens == null || !itens.Any())
                return BadRequest(new { Message = "O carrinho está vazio." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"UserId from token: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Utilizador não identificado. Faça login novamente." });
            }

            var cliente = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
            {
                _logger.LogWarning($"Cliente não encontrado para userId: {userId}");
                
                // Verificar se o utilizador existe
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    _logger.LogInformation($"User existe: {user.Email}, Perfil: {user.Perfil}");
                    
                    // Se o utilizador existe mas não tem registo de Cliente, criar automaticamente
                    if (user.Perfil == "Cliente")
                    {
                        _logger.LogInformation("A criar registo de Cliente automaticamente...");
                        cliente = new Cliente
                        {
                            ApplicationUserId = userId,
                            NIF = "9" + new Random().Next(10000000, 99999999).ToString(),
                            Estado = "Ativo"
                        };
                        _context.Clientes.Add(cliente);
                        await _context.SaveChangesAsync();
                        
                        // Recarregar com ApplicationUser
                        cliente = await _context.Clientes
                            .Include(c => c.ApplicationUser)
                            .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
                    }
                    else
                    {
                        return BadRequest(new { Message = $"O seu perfil é '{user.Perfil}'. Apenas Clientes podem fazer compras." });
                    }
                }
                else
                {
                    return Unauthorized(new { Message = "Utilizador não encontrado. Faça login novamente." });
                }
            }

            if (cliente == null)
            {
                return BadRequest(new { Message = "Não foi possível criar o registo de cliente." });
            }

            // Verificar se a conta do utilizador está ativa
            if (cliente.ApplicationUser?.Estado != "Ativo")
            {
                _logger.LogWarning($"Conta não ativa. Estado: {cliente.ApplicationUser?.Estado}");
                return BadRequest(new { Message = "A sua conta não está ativa." });
            }

            // Validar todos os produtos antes de criar a venda
            foreach (var item in itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                if (produto == null)
                    return BadRequest(new { Message = $"Produto com ID {item.ProdutoId} não existe." });

                if (produto.Estado != "Ativo")
                    return BadRequest(new { Message = $"Produto '{produto.Nome}' não está disponível." });

                if (produto.Stock < item.Quantidade)
                    return BadRequest(new { Message = $"Stock insuficiente para o produto '{produto.Nome}'. Disponível: {produto.Stock}" });
            }

            // Criar nova venda - Estado Pendente aguarda confirmação do admin
            var venda = new Venda
            {
                ClienteId = cliente.Id,
                Estado = "Pendente",
                Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            decimal total = 0;

            foreach (var item in itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);

                _context.LinhasVenda.Add(new LinhaVenda
                {
                    VendaId = venda.Id,
                    ProdutoId = produto!.Id,
                    Quantidade = item.Quantidade,
                    Preco = produto.PrecoFinal
                });

                total += produto.PrecoFinal * item.Quantidade;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Venda criada com sucesso. ID: {venda.Id}, Total: {total}");

            return Ok(new
            {
                Message = "Encomenda criada com sucesso! Aguarda confirmação.",
                VendaId = venda.Id,
                Total = total,
                Estado = venda.Estado
            });
        }

        /// <summary>
        /// Cliente consulta o histórico das suas compras
        /// </summary>
        [HttpGet("minhas")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MinhasVendas()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"MinhasVendas - UserId: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Utilizador não identificado." });
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
            {
                _logger.LogWarning($"Cliente não encontrado para userId: {userId}");
                
                // Verificar se o utilizador existe e é Cliente
                var user = await _context.Users.FindAsync(userId);
                if (user != null && user.Perfil == "Cliente")
                {
                    // Criar registo de cliente automaticamente
                    cliente = new Cliente
                    {
                        ApplicationUserId = userId,
                        NIF = "9" + new Random().Next(10000000, 99999999).ToString(),
                        Estado = "Ativo"
                    };
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return Ok(new List<object>()); // Retorna lista vazia
                }
            }

            var vendas = await _context.Vendas
                .Where(v => v.ClienteId == cliente.Id)
                .Include(v => v.LinhasVenda!)
                    .ThenInclude(l => l.Produto)
                .OrderByDescending(v => v.Data)
                .ToListAsync();

            _logger.LogInformation($"Encontradas {vendas.Count} vendas para cliente {cliente.Id}");

            var result = vendas.Select(v => new
            {
                Id = v.Id,
                Data = v.Data,
                Estado = v.Estado,
                Total = v.LinhasVenda?.Sum(l => l.Preco * l.Quantidade) ?? 0,
                Linhas = v.LinhasVenda?.Select(l => new
                {
                    ProdutoId = l.ProdutoId,
                    ProdutoNome = l.Produto?.Nome ?? "Produto",
                    ProdutoImagem = l.Produto?.Imagem ?? "semfoto.png",
                    Quantidade = l.Quantidade,
                    PrecoUnitario = l.Preco,
                    Subtotal = l.Preco * l.Quantidade
                }).ToList()
            });

            return Ok(result);
        }

        /// <summary>
        /// Regista intenção de pagamento (a venda continua Pendente até admin confirmar)
        /// </summary>
        [HttpPost("{id}/pagar")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> RegistarPagamento(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cliente == null)
                return Unauthorized(new { Message = "Cliente não encontrado." });

            var venda = await _context.Vendas
                .Include(v => v.LinhasVenda)
                .FirstOrDefaultAsync(v => v.Id == id && v.ClienteId == cliente.Id);

            if (venda == null)
                return NotFound(new { Message = "Venda não encontrada." });

            if (venda.Estado != "Pendente")
                return BadRequest(new { Message = "Esta venda já foi processada." });

            // A venda permanece Pendente - Admin/Funcionário irá confirmar na GestaoLoja
            var total = venda.LinhasVenda?.Sum(l => l.Preco * l.Quantidade) ?? 0;

            return Ok(new
            {
                Success = true,
                Message = "Pagamento registado! A sua encomenda aguarda confirmação.",
                VendaId = venda.Id,
                Total = total,
                Estado = venda.Estado
            });
        }

        /// <summary>
        /// Debug - verificar estado do cliente (REMOVER EM PRODUÇÃO)
        /// </summary>
        [HttpGet("debug/cliente-status")]
        [Authorize]
        public async Task<IActionResult> DebugClienteStatus()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            var user = await _context.Users.FindAsync(userId);
            var cliente = await _context.Clientes
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            return Ok(new
            {
                UserId = userId,
                Email = userEmail,
                Roles = userRoles,
                UserExists = user != null,
                UserPerfil = user?.Perfil,
                UserEstado = user?.Estado,
                ClienteExists = cliente != null,
                ClienteId = cliente?.Id,
                ClienteEstado = cliente?.Estado
            });
        }
    }

    /// <summary>
    /// DTO para item de venda
    /// </summary>
    public class ItemVendaDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}
