# Venice Orders - Teste Técnico

## Visão Geral

Este projeto implementa um sistema modular para gerenciamento de pedidos (**Orders**) baseado em uma arquitetura limpa (**Clean Architecture**) e DDD (**Domain-Driven Design**). O objetivo é garantir uma aplicação com alta manutenibilidade, separação clara de responsabilidades, testabilidade e baixo acoplamento entre camadas.

O projeto contém:

- **API REST** (Camada de apresentação)
- **Camada de Aplicação**
- **Camada de Domínio**
- **Infraestrutura**
- **Testes Unitários** com xUnit + Moq
- Orquestração via **Docker Compose**
- Mensageria utilizando **RabbitMQ**

---

## Decisões de Arquitetura

A arquitetura segue uma estrutura inspirada em **Clean Architecture**, onde cada camada possui responsabilidades bem definidas.

### **1. API (src/Venice.Orders.Api)**
- Exposição de endpoints REST.
- Validações de entrada.
- Injeta serviços definidos na camada de aplicação.
- Sem regras de negócio.

### **2. Application (src/Venice.Orders.Application)**
- Contém os casos de uso.
- Contém serviços de aplicação, DTOs e handlers.
- Orquestra lógica usando o domínio, mas não implementa regras de negócio puras.
- Define interfaces que serão implementadas pela camada de Infraestrutura.

### **3. Domain (src/Venice.Orders.Domain)**
- **Núcleo da aplicação**.
- Entidades, Value Objects, regras de negócio.
- Totalmente independente de detalhes externos.
- Não conhece banco de dados, API, RabbitMQ, etc.

### **4. Infrastructure (src/Venice.Orders.Infrastructure)**
- Implementação de repositórios, persistência e integrações.
- Comunicações externas (RabbitMQ, banco, APIs externas).
- Implementa as interfaces definidas na camada Application.
- Pode ser substituída sem impactar o domínio.

### **5. Testes Unitários (tests/Venice.Orders.UnitTests)**
- xUnit para execução dos testes.
- Moq para mocks/stubs.
- Foco na lógica da camada Application e Domain.
- Configurado para rodar automaticamente via GitHub Actions.

---

## Dependências do Projeto

### Tecnologias principais
- **.NET 7**
- **ASP.NET Core**
- **RabbitMQ**
- **Docker + Docker Compose**

### Bibliotecas de apoio
- xUnit  
- Microsoft.NET.Test.Sdk  

---

## Rodando com Docker

### Pré-requisitos
- Docker  
- Docker Compose  

---

### 1. Subir o ambiente

```bash
docker compose up -d
```

**IMPORTANTE:** Na primeira vez, o Docker irá baixar todas as imagens necessárias (.NET SDK, runtime, RabbitMQ, etc). Este processo pode levar vários minutos.

---

### 2. Acessando a API

```
http://localhost:5000
```

(Verifique a porta no arquivo docker-compose.yml)

---

### Acessando o RabbitMQ

```
http://localhost:15672
```

- user: guest  
- password: guest  

---

### 3. Logs

```bash
docker compose logs -f
```

---

### 4. Derrubar containers

```bash
docker compose down
```

---

## Rodando os Testes

Localmente:

```bash
dotnet test
```

No GitHub, o workflow `run-tests.yml` executa automaticamente:

- push para `main`
- pull request para `main`

---

## Estrutura do Projeto

```
├── src
│   ├── Venice.Orders.Api
│   ├── Venice.Orders.Application
│   ├── Venice.Orders.Domain
│   └── Venice.Orders.Infrastructure
│
├── tests
│   └── Venice.Orders.UnitTests
│
├── docker-compose.yml
├── Dockerfile
├── README.md
└── Venice.Orders.sln
```

---

## Conclusão

Este projeto foi construído para ser modular, robusto e fácil de manter.  
A arquitetura Clean Architecture com DDD garante:

- maior testabilidade  
- separação forte entre camadas  
- alta possibilidade de evolução  
- facilidade para substituir implementações sem refatorar regras de negócio  
