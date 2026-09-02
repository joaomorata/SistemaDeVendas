# 🚚 SistemaDeVendas

Aplicação web em **ASP.NET Core MVC (.NET 8)** para gestão de **clientes, pacotes e rotas de entrega/coleta**, com autenticação de usuários via **ASP.NET Core Identity** e **otimização automática de rotas** a partir das coordenadas geográficas dos clientes.

O projeto nasceu como um CRUD simples de vendas (`TelaLoginCrud`) e evoluiu para um sistema de logística: cadastra-se o cliente, o endereço é geocodificado, os pacotes são vinculados a ele e uma rota é montada escolhendo os clientes a visitar — o sistema calcula a melhor ordem de visita e a distância total.

---

## 📋 Tecnologias

| Camada | Stack |
|---|---|
| Backend | C# / .NET 8 / ASP.NET Core MVC |
| Autenticação | ASP.NET Core Identity (Razor Pages) |
| ORM | Entity Framework Core 8 (Code First + Migrations) |
| Banco de dados | PostgreSQL (Npgsql) |
| Geocodificação | API pública [Nominatim / OpenStreetMap](https://nominatim.openstreetmap.org/) |
| Frontend | Razor Views, Bootstrap 5, Bootstrap Icons, jQuery |
| Deploy | Docker + [Render](https://render.com) |

### Pacotes NuGet

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Identity.UI`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.VisualStudio.Web.CodeGeneration.Design`

---

## 🚀 Funcionalidades

### Autenticação
- Cadastro, login, logout e redefinição de senha (ASP.NET Core Identity)
- Usuário estendido com `Nome` e `Sobrenome`
- Todos os controllers de negócio exigem usuário autenticado (`[Authorize]`)

### Clientes
- CRUD completo
- Geocodificação automática do endereço no cadastro/edição (latitude/longitude via Nominatim)
- Cada cliente possui uma lista de pacotes

### Pacotes
- CRUD completo, vinculado a um cliente
- Controle de peso (kg) e situação de coleta (`Coletado`)

### Rotas
- Criação de rota a partir de um **endereço de partida** e da seleção de **clientes com coordenadas**
- **Otimização automática da ordem de visita** (`Services/OtimizadorRota.cs`):
  - Heurística do **vizinho mais próximo** para a solução inicial
  - Refinamento **2-opt**
  - Distâncias calculadas pela fórmula de **Haversine** (km)
- Fluxo de status: `Planejada → EmAndamento → Concluída`
- Registro de coleta por parada: marca a parada como visitada, marca os pacotes do cliente como coletados e conclui a rota automaticamente quando todas as paradas são visitadas

### Vendas (módulo original)
- CRUD de vendas (produto, quantidade, preço, data)

### Operação
- Endpoint de health check em `GET /healthz`
- Migrations aplicadas automaticamente no startup (`db.Database.Migrate()`)
- Suporte a proxy reverso via `ForwardedHeaders` (deploy atrás do Render)

---

## 🗄 Banco de dados

Modelagem **Code First** com Migrations do EF Core. Entidades principais:

- `Usuario` (Identity) · `Venda` · `Cliente` · `Pacote` · `Rota` · `RotaParada`
- Enum `StatusRota` (`Planejada`, `EmAndamento`, `Concluida`)

A string de conexão é resolvida nesta ordem (ver `Program.cs`):

1. Variável de ambiente **`DATABASE_URL`** no formato URL
   (`postgresql://usuario:senha@host:porta/banco`) — usada em produção (Render/Supabase);
   convertida internamente para o formato chave=valor do Npgsql, com `SslMode=Require`.
2. `ConnectionStrings:SistemaDeVendasContextConnection`
   (env `ConnectionStrings__SistemaDeVendasContextConnection`, `appsettings.json` ou **user-secrets**).

Se nenhuma for encontrada, a aplicação lança exceção no startup.

---

## ▶️ Como executar localmente

### Pré-requisitos
- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- PostgreSQL em execução (local ou em container)
- (Opcional) [EF Core CLI](https://learn.microsoft.com/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

### Passos

```bash
# 1. Clonar
git clone https://github.com/joaomorata/TelaLoginCrud.git
cd TelaLoginCrud

# 2. Configurar a conexão via user-secrets (recomendado)
cd SistemaDeVendas
dotnet user-secrets set "ConnectionStrings:SistemaDeVendasContextConnection" \
  "Host=localhost;Port=5432;Database=sistemadevendas;Username=postgres;Password=SUA_SENHA"

# 3. Aplicar as migrations (o startup também faz isso automaticamente)
dotnet ef database update

# 4. Rodar
dotnet run
```

A aplicação sobe em `http://localhost:5112` (perfil `http` de `launchSettings.json`).

> Alternativa à etapa 2: definir a variável de ambiente
> `DATABASE_URL=postgresql://postgres:SUA_SENHA@localhost:5432/sistemadevendas`.

---

## 🐳 Docker

```bash
docker build -t sistemadevendas .
docker run -p 10000:10000 -e DATABASE_URL="postgresql://user:senha@host:5432/banco" sistemadevendas
```

O `Dockerfile` publica em Release, expõe a porta **10000** (`ASPNETCORE_URLS=http://+:10000`) e desativa o file watcher de configuração (limite de inotify baixo em containers do Render).

---

## ☁️ Deploy no Render

- Serviço do tipo **Web Service** usando o `Dockerfile` do repositório
- Banco **PostgreSQL** (Render ou Supabase); definir a variável `DATABASE_URL`
- Health check path: `/healthz`
- As migrations rodam sozinhas na primeira inicialização

---

## 📂 Estrutura do projeto

```text
SistemaDeVendas/
├── Areas/Identity/            # Autenticação (Identity + Razor Pages)
│   ├── Data/
│   │   ├── SistemaDeVendasContext.cs   # DbContext (IdentityDbContext<Usuario>)
│   │   └── Usuario.cs
│   └── Pages/Account/         # Login, Register, Logout, ResetPassword...
├── Controllers/
│   ├── HomeController.cs
│   ├── ClienteController.cs
│   ├── PacoteController.cs
│   ├── RotaController.cs
│   └── VendaController.cs
├── Models/
│   ├── Cliente.cs  Pacote.cs  Rota.cs  RotaParada.cs  StatusRota.cs
│   ├── Venda.cs
│   ├── RotaCreateViewModel.cs
│   └── ErrorViewModel.cs
├── Services/
│   ├── GeocodingService.cs    # Consulta ao Nominatim/OpenStreetMap
│   └── OtimizadorRota.cs      # Vizinho mais próximo + 2-opt + Haversine
├── Migrations/                # EF Core (InitialPg)
├── Views/                     # Razor Views (Home, Cliente, Pacote, Rota, Venda, Shared)
├── wwwroot/                   # css, js, libs estáticas
├── appsettings.json
├── Program.cs                 # Bootstrap, resolução de conexão, DI, pipeline
└── Dockerfile
```

---

## 📷 Telas

| Login | Cadastro de usuário |
|---|---|
| ![Login](SistemaDeVendas/imagens/tela-inicial.png) | ![Cadastro](SistemaDeVendas/imagens/tela_cadastro.png) |

| Painel de vendas | Cadastro de venda |
|---|---|
| ![Painel de vendas](SistemaDeVendas/imagens/painel-venda.png) | ![Cadastro de venda](SistemaDeVendas/imagens/cadastro-venda.png) |

---

## 👨‍💻 Autores

**Desenvolvedor:** 
João Pedro Rabelo Schoettner Morata
Manoel Almeida de Morais

