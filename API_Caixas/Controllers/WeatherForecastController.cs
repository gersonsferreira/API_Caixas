using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;

namespace API_Caixas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        // Modelos de caixas disponíveis
        private static readonly List<Caixa> CaixasDisponiveis = new List<Caixa>
        {
            new Caixa { Altura = 30, Largura = 40, Comprimento = 80 },
            new Caixa { Altura = 80, Largura = 50, Comprimento = 40 },
            new Caixa { Altura = 50, Largura = 80, Comprimento = 60 }
        };

        [HttpPost]
        public IActionResult ReceberPedidos([FromBody] List<PedidosCab> pedidos)
        {
            if (pedidos == null || pedidos.Count == 0)
            {
                return BadRequest("Pedidos inválidos.");
            }

            var resultado = new List<RespostaPedido>();

            foreach (var pedidosSub in pedidos)
            {
                foreach (var pedido in pedidosSub.pedidos)
                {
                    var caixas = ProcessarPedido(pedido);
                    List<string> produtosResposta = new List<string>();
                    foreach (var produto in pedido.Produtos)
                    {
                        produtosResposta.Add(produto.produto_id);
                    }
                    resultado.Add(new RespostaPedido
                    {
                        pedido_id = pedido.pedido_id,
                        Caixas = caixas.Select((c, index) => new CaixaResposta
                        {
                            caixa_id = "Caixa " + index + 1,
                            produtos = produtosResposta
                        }).ToList()
                    });
                }
            }
            return Ok(resultado);
        }

        private List<Caixa> ProcessarPedido(Pedido pedido)
        {
            // Lógica para otimização do empacotamento
            List<Caixa> caixasUsadas = new List<Caixa>();

            foreach (var produto in pedido.Produtos)
            {
                bool produtoEmpacotado = false;

                // Tentar adicionar o produto em uma caixa já existente
                foreach (var caixa in caixasUsadas)
                {
                    if (caixa.CabeProduto(produto))
                    {
                        caixa.Produtos.Add(produto);
                        produtoEmpacotado = true;
                        break;
                    }
                }

                // Se o produto não coube em nenhuma caixa existente, usar uma nova caixa
                if (!produtoEmpacotado)
                {
                    foreach (var caixaDisponivel in CaixasDisponiveis)
                    {
                        if (caixaDisponivel.CabeProduto(produto))
                        {
                            var novaCaixa = new Caixa
                            {
                                Altura = caixaDisponivel.Altura,
                                Largura = caixaDisponivel.Largura,
                                Comprimento = caixaDisponivel.Comprimento
                            };
                            novaCaixa.Produtos.Add(produto);
                            caixasUsadas.Add(novaCaixa);
                            break;
                        }
                    }
                }
            }

            return caixasUsadas;
        }
    } 
    
    public class PedidosCab
    {
        [JsonProperty("pedidos")]
        public List<Pedido> pedidos { get; set; }
    }

    public class Pedido
    {
        [JsonProperty("pedido_id")]
        public int pedido_id { get; set; }

        [JsonProperty("produtos")]
        public List<Produto> Produtos { get; set; }
    }

    public class Produto
    {
        [JsonProperty("produto_id")]
        public string produto_id { get; set; }

        [JsonProperty("dimensoes")]
        public Dimensoes Dimensoes { get; set; }
    }

    public class Dimensoes
    {
        [JsonProperty("altura")]
        public double Altura { get; set; }

        [JsonProperty("largura")]
        public double Largura { get; set; }

        [JsonProperty("comprimento")]
        public double Comprimento { get; set; }
    }

    public class Caixa
    {
        public double Altura { get; set; }
        public double Largura { get; set; }
        public double Comprimento { get; set; }
        public List<Produto> Produtos { get; set; } = new List<Produto>();

        public bool CabeProduto(Produto produto)
        {
            // Lógica simplificada para verificar se o produto cabe na caixa
            return produto.Dimensoes.Altura <= Altura &&
                   produto.Dimensoes.Largura <= Largura &&
                   produto.Dimensoes.Comprimento <= Comprimento;
        }
    }

    public class RespostaPedido
    {
        [JsonProperty("pedido_id")]
        public int pedido_id { get; set; }

        [JsonProperty("caixas")]
        public List<CaixaResposta> Caixas { get; set; }
    }

    public class CaixaResposta
    {
        [JsonProperty("caixa_id")]
        public string caixa_id { get; set; }

        [JsonProperty("produtos")]
        public List<String> produtos { get; set; }
    }

}
