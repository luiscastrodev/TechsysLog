# 📄 Resumo Técnico - TechsysLog

## Olá! 👋

Este documento apresenta uma visão geral das decisões técnicas e tecnologias utilizadas no desenvolvimento do **TechsysLog** - Sistema de Controle de Pedidos e Entregas.

---

## 🎯 Contexto do Projeto

O TechsysLog foi desenvolvido como teste técnico com objetivo de criar uma plataforma completa de logística que permite:

- ✅ **Cadastro de usuários** (Clientes e Operadores)
- ✅ **Gerenciamento de pedidos** com rastreamento de status
- ✅ **Integração com API externa** (ViaCEP) para validação de endereços
- ✅ **Notificações em tempo real** via SignalR
- ✅ **Autenticação e autorização** com JWT
- ✅ **Interface responsiva** para desktop e mobile

---

## 🛠️ Stack Tecnológico Escolhido

### Backend: ASP.NET Core 8.0+

**Por que ASP.NET Core?**
- Requisito obrigatório do teste ✅
- Framework robusto e maduro
- Excelente performance
- Suporte nativo a SignalR
- Grande comunidade e documentação

**Dependências Principais:**
- **Entity Framework Core** - ORM para acesso a dados
- **SignalR** - Comunicação em tempo real via WebSockets
- **JWT Bearer** - Autenticação segura com tokens
- **BCrypt.Net** - Hash criptográfico de senhas
- **MongoDB Driver** - Acesso ao banco NoSQL

### Frontend: Angular 15+

**Por que Angular?**
- Requisito desejável do teste ✅
- Framework completo com CLI
- Tipagem strong com TypeScript
- Excelente para aplicações enterprise
- Componentes reutilizáveis

**Dependências Principais:**
- **RxJS** - Programação reativa e observables
- **TypeScript** - Tipagem estática
- **Tailwind CSS** - Estilização utilitária
- **@microsoft/signalr** - Cliente SignalR
- **HttpClient** - Requisições HTTP

### Banco de Dados: MongoDB

**Por que MongoDB?**
- Requisito desejável do teste ✅
- Banco NoSQL flexível
- Fácil de escalar horizontalmente
- Schema dinâmico (perfeito para prototipagem)
- Bom suporte com Entity Framework Core

**Collections:**
- `Users` - Usuários do sistema
- `Orders` - Pedidos criados
- `OrderHistories` - Histórico de mudanças de status
- `Deliveries` - Registros de entrega
- `Notifications` - Notificações do sistema
- `RefreshTokens` - Tokens de refresh para renovação

---

## 🏗️ Arquitetura Implementada

### Clean Architecture em 4 Camadas

Implementei Clean Architecture para garantir:
- ✅ Separação clara de responsabilidades
- ✅ Fácil testabilidade
- ✅ Manutenibilidade de longo prazo
- ✅ Independência de frameworks

```
┌─────────────────────────────────────┐
│  TechsysLog.Api (Controllers)       │  ← Apresentação
├─────────────────────────────────────┤
│  TechsysLog.Application (Services)  │  ← Aplicação
├─────────────────────────────────────┤
│  TechsysLog.Domain (Entidades)      │  ← Domínio
├─────────────────────────────────────┤
│  TechsysLog.Infrastructure (Data)   │  ← Infraestrutura
└─────────────────────────────────────┘
```

### Padrões de Design Implementados

1. **Repository Pattern**
   - Abstração de acesso a dados
   - Facilita testes com mocks
   - Implementação genérica reutilizável

2. **Dependency Injection**
   - Container DI nativo do .NET
   - Inversão de controle
   - Acoplamento reduzido

3. **DTO Pattern**
   - Segurança (não expõe entidades inteiras)
   - Controle de dados transferidos
   - Independência entre front e back

4. **Business Result Pattern**
   - Respostas padronizadas
   - Tratamento consistente de erros
   - Tipo retornado sempre igual

```csharp
public class BusinessResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; }
}
```

5. **Mapper Pattern**
   - Transformação Entidade → DTO
   - Extensões para facilitar conversão
   - Sem dependências externas

6. **Observer Pattern** (SignalR)
   - Notificações em tempo real
   - Desacoplamento entre componentes
   - Escalabilidade

---

## 🔐 Segurança Implementada

### Autenticação JWT

```
Cliente                          Servidor
   │                                │
   │──── POST /auth/login ────────>│
   │     (email, password)         │
   │                                │
   │<── AccessToken + RefreshToken│
   │     (JWT válido por 3h)       │
   │                                │
   │──── GET /api/orders ─────────>│
   │     (Header: Bearer Token)    │
   │                                │
   │<──── Dados autorizados ──────│
```

### Account Lockout

Após 5 tentativas de login falhas:
- Conta bloqueada por 15 minutos
- Mensagem clara ao usuário
- Proteção contra brute force

### Password Hashing

Senhas criptografadas com **BCrypt**:
- One-way hashing (não reversível)
- Salt aleatório
- Computacionalmente custoso

### CORS Restritivo

```csharp
options.AddPolicy("CorsPolicy", builder =>
{
    builder
        .WithOrigins("http://localhost:4200")  // Apenas frontend
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();  // Necessário para SignalR
});
```

### Soft Delete

Dados nunca são deletados, apenas marcados:
```csharp
public async Task DeleteAsync(Guid id)
{
    var entity = await GetByIdAsync(id);
    entity.Deleted = true;  // Soft delete
    await UpdateAsync(entity);
}
```

---

## 🔔 Notificações em Tempo Real (SignalR)

### Por que SignalR?

- ✅ Integrado ao ASP.NET Core
- ✅ Suporta múltiplos transportes (WebSocket, SSE, LongPolling)
- ✅ Reconexão automática
- ✅ Escalável
- ✅ Cross-browser compatibility

### Fluxo de Funcionamento

```
1. Usuário faz login
   └─> Frontend se conecta ao Hub SignalR

2. Usuário cria pedido
   └─> API notifica via SignalR → Frontend recebe em tempo real

3. Operador altera status
   └─> API notifica cliente → Painel atualiza automaticamente

4. Entrega registrada
   └─> API notifica cliente → Badge de notificação aparece
```

### Eventos Implementados

| Evento | Descrição | Quando Ocorre |
|--------|-----------|---------------|
| `ReceiveNotification` | Notificação genérica | Sempre que há notificação |
| `OrderStatusChanged` | Status alterado | Operador muda status |
| `OrderDelivered` | Pedido entregue | Operador registra entrega |
| `NewOrderCreated` | Novo pedido criado | Cliente cria pedido |

---

## 📊 Fluxo de Dados Completo

### Exemplo: Criar Pedido

```
┌──────────────────────┐
│   Navegador (4200)   │
│   - Formulário       │
│   - Validações       │
└──────────┬───────────┘
           │ POST /api/orders
           │ {description, amount, address}
           ↓
┌──────────────────────────────┐
│  ASP.NET Core (7071)         │
│  OrdersController            │
│  - Extrai userId do JWT      │
│  - Valida DTO                │
└──────────┬───────────────────┘
           │
           ↓
┌──────────────────────────────┐
│  OrderService                │
│  - Integra com ViaCEP        │
│  - Cria entidade Order       │
│  - Persiste no banco         │
│  - Cria Notification         │
│  - Notifica via SignalR      │
└──────────┬───────────────────┘
           │
           ├─────────────────┬────────────────┐
           ↓                 ↓                ↓
    ┌────────────┐   ┌────────────┐   ┌────────────┐
    │ MongoDB    │   │ SignalR    │   │ Response   │
    │ (Salva)    │   │ (Notifica) │   │ (HTTP 200) │
    └────────────┘   └────────────┘   └────────────┘
           │                 │                │
           │                 │                │
           └─────────────────┴────────────────┘
                      │
                      ↓ (BehaviorSubject)
           ┌──────────────────────────┐
           │ Navegador (4200)         │
           │ - Dashboard atualizado   │
           │ - Notificação recebida   │
           │ - Contador atualizado    │
           └──────────────────────────┘
```

---

## 🌐 Integração com ViaCEP

Para validação automática de endereços:

```csharp
public async Task<Address?> GetAddressByCepAsync(string zipcode)
{
    var response = await _httpClient
        .GetFromJsonAsync<ViaCepResponseDto>(
            $"https://viacep.com.br/ws/{zipcode}/json/"
        );
    
    return new Address
    {
        ZipCode = response.cep,
        Street = response.logradouro,
        Neighborhood = response.bairro,
        City = response.localidade,
        State = response.uf
    };
}
```

**Benefícios:**
- ✅ Evita endereços inválidos
- ✅ Preenchimento automático
- ✅ Melhor UX
- ✅ Dados consistentes

---


## 📈 Escalabilidade

### Implementado

1. **Database Indexing**
   - Index em `UserId` para queries rápidas
   - Index único em `OrderNumber`

2. **Lazy Loading** (Angular)
   - Módulos carregados sob demanda
   - Reduz bundle size inicial

3. **Change Detection OnPush**
   - Angular detecta mudanças apenas quando necessário
   - Melhor performance


## 📚 Documentação

Para melhor entender o projeto, consulte:

1. **[README.md](../README.md)**
   - Overview geral
   - Screenshots
   - Instruções de instalação


---

## ✨ Destaques Técnicos

### 1. Clean Architecture
Implementação correta com 4 camadas bem separadas, garantindo testabilidade e manutenibilidade.

### 2. SignalR em Tempo Real
Notificações instantâneas sem polling, utilizando WebSockets com fallback automático.

### 3. Segurança em Camadas
- JWT para autenticação
- BCrypt para senhas
- CORS restritivo
- Account Lockout
- Soft Delete

### 4. DTOs Pattern
Nunca exponho entidades diretamente, garantindo segurança e flexibilidade.

### 5. MongoDB com EF Core
Usa LINQ mesmo em NoSQL, mantendo familiaridade com SQL Server.

### 6. API RESTful Completa
Endpoints bem estruturados, documentados com Swagger, seguindo boas práticas.

### 7. Frontend Reativo
RxJS Observables com proper unsubscription, evitando memory leaks.

### 8. Docker Ready
Setup completo com Docker Compose para fácil onboarding.

---

## 🎯 Requisitos Atendidos

### Requisitos Obrigatórios ✅

- ✅ **ASP.NET Core** - Backend em .NET 7.0+
- ✅ **API REST** - Endpoints RESTful bem documentados
- ✅ **GitHub Público** - https://github.com/luiscastrodev/TechsysLog

### Requisitos Desejáveis ✅

- ✅ **MongoDB** - Banco de dados NoSQL implementado
- ✅ **Angular** - Frontend com Angular 16+
- ✅ **SignalR** - Notificações em tempo real

### Funcionalidades Técnicas ✅

- ✅ Cadastro de usuários
- ✅ Cadastro de pedidos
- ✅ Integração com ViaCEP
- ✅ Autenticação JWT
- ✅ Notificações em tempo real
- ✅ Histórico de pedidos
- ✅ Sistema de roles (Client, Operator, Admin)
- ✅ Soft delete
- ✅ Account lockout

---

## 💪 Competências Demonstradas

### Architecture & Design
- ✅ Clean Architecture
- ✅ SOLID Principles
- ✅ Design Patterns (Repository, Dependency Injection, etc)
- ✅ Domain-Driven Design concepts

### Backend
- ✅ ASP.NET Core avançado
- ✅ Entity Framework Core
- ✅ SignalR
- ✅ RESTful API Design
- ✅ Authentication & Authorization

### Frontend
- ✅ Angular (Standalone Components)
- ✅ RxJS (Observables, Operators)
- ✅ TypeScript (Strong Typing)
- ✅ Reactive Forms
- ✅ Component Architecture
- ✅ SignalR Client

### Dados
- ✅ MongoDB
- ✅ NoSQL Concepts
- ✅ Data Modeling
- ✅ Indexing


### Boas Práticas
- ✅ Code Organization
- ✅ Naming Conventions
- ✅ Error Handling
- ✅ Documentation

---

## 🎓 Aprendizados e Decisões

### Por que essas escolhas?

1. **Clean Architecture**
   - Prepara o projeto para escala
   - Facilita onboarding de novos devs
   - Testes mais fáceis

2. **SignalR em vez de WebSockets puros**
   - Abstração de transporte
   - Reconexão automática
   - Melhor DX

3. **DTOs em tudo**
   - Segurança
   - Independência entre front/back
   - Controle de versioning

4. **MongoDB via EF Core**
   - Aproveita conhecimento SQL
   - LINQ para queries
   - Mesma abstração
 Production-like local

---

## 📞 Contato

Repositório: https://github.com/luiscastrodev/TechsysLog
---

<div align="center">

**Obrigado por avaliar o TechsysLog!** 🙏

Desenvolvido com ❤️ e atenção ao detalhe

</div>
