# Plano técnico

## Contexto
Implementa a especificação do encurtador de links, respeitando a constituição
do projeto. Em qualquer conflito, a constituição prevalece.

## Arquitetura
- A aplicação segue Vertical Slice Architecture (VSA): cada caso de uso agrupa
  o seu endpoint Minimal API, contratos HTTP, handler de aplicação e testes
- `Features/Links/Create` trata criação automática e personalizada, validação
  de URL e expiração; `Features/Links/Redirect` trata acesso e redireccionamento;
  `Features/Links/Stats` consulta estatísticas
- Os handlers são serviços de aplicação por caso de uso: contêm as regras de
  negócio e chamam o repositório; não são handlers HTTP
- Os endpoints fazem binding e tradução HTTP e delegam no handler da sua fatia;
  não contêm regras de negócio nem acedem directamente à base de dados
- `Domain/Links` contém o agregado `Link`; `Infrastructure/Persistence` contém
  o `DbContext` e um repositório para esse agregado, partilhado pelas fatias
- A fatia `Create` contém a geração de códigos; regras de domínio partilhadas
  ficam em `Domain/Links`, enquanto o repositório mantém invariantes de
  persistência, sem um `LinkService` monolítico
- Não introduzir Mediator, dispatcher genérico ou dependências novas para
  implementar VSA
- Os handlers obtêm o instante atual através de `TimeProvider`, para usar UTC
  de forma consistente e permitir testes determinísticos; não requer dependência nova

## Dependências e justificativas
- `Microsoft.EntityFrameworkCore.Sqlite`: necessária para persistir os links em
  SQLite (RF-08, D-01) e executar a actualização atómica do contador (D-02)
- `Microsoft.AspNetCore.Mvc.Testing`: necessária para testar por HTTP os endpoints
  Minimal API e cumprir a cobertura de integração exigida pela constituição
- `Microsoft.NET.Test.Sdk`, `xunit` e `xunit.runner.visualstudio`: necessários
  para executar os testes xUnit exigidos pela constituição
- Não se adiciona EF Design nem `dotnet-ef`: a migração inicial é versionada no
  código e validada através de `Database.Migrate()` nos testes
- `TimeProvider` pertence ao framework .NET 10; não requer pacote adicional
- Não se introduzem Mediator, dispatcher ou bibliotecas de validação

## Decisões
- D-01: SQLite como armazenamento, porque RF-08 exige persistência entre reinícios
- D-02: cada acesso válido incrementa `AccessCount` com um `UPDATE` atómico no
  banco, nunca com leitura e reescrita na aplicação; a actualização verifica a
  expiração contra o mesmo instante UTC usado para decidir se o acesso é válido,
  atendendo a RF-05, RN-03 e RN-04
- D-03: os códigos automáticos têm 7 caracteres sorteados de `A-Z`, `a-z` e
  `1-9`, conforme RF-02
- D-04: um índice único com comparação sensível a maiúsculas/minúsculas garante
  RN-01 além da verificação no handler de criação; colisão de código personalizado resulta
  em `400 ProblemDetails`, por ser erro de validação segundo a constituição, e
  colisão automática gera outro código até a inserção ter sucesso
- D-05: `shortUrl` é construído com o esquema e o host externos do pedido de
  criação, seguidos de `/{code}`, conforme RF-01; cabe ao deployment configurar
  hosts permitidos e confiar em cabeçalhos de proxy apenas de proxies conhecidos
- D-06: as estatísticas são públicas para qualquer pessoa que conheça o código,
  conforme RF-07; não existe chave de gestão nem autenticação
- D-07: todos os erros de validação, incluindo código personalizado duplicado,
  URL inválida e expiração passada, respondem `400 ProblemDetails`, conforme a
  constituição, com mensagens em português

## Modelo de dados
`Link`
- `Id`, `Code` (único), `TargetUrl`, `CreatedAtUtc`, `ExpiresAtUtc` (nulo),
  `AccessCount`
- Não existe chave de gestão: as estatísticas são públicas

## Contratos HTTP
- `POST /api/links`
  - Entrada: `targetUrl`, `code` opcional, `expiresAtUtc` opcional (ISO 8601 UTC
    terminado em `Z`)
  - Sucesso: `201` com `{ shortUrl, code }`; `shortUrl` usa o esquema e host
    externos do pedido
  - Erro de validação: `400 ProblemDetails`
- `GET /{code}`
  - Link válido: incrementa o contador uma vez e responde `302` para `TargetUrl`
  - Código inexistente: `404`; link expirado: `410`, sem incrementar o contador
- `GET /api/links/{code}/stats`
  - Sem autenticação ou header de chave
  - Código existente, mesmo expirado: `200` com `{ accessCount }`
  - Código inexistente: `404`

## Cobertura de qualidade
- Cada regra de negócio RN-01 a RN-07 tem testes unitários
- Cada endpoint tem um teste de integração de caminho feliz: criação `201`,
  redireccionamento `302` e estatísticas `200`
- Testes de integração verificam erros de validação em `400 ProblemDetails`,
  redireccionamento de link expirado em `410`, link inexistente em `404`,
  contagem concorrente e persistência após reinício

## Riscos
- A geração automática repete até inserir um código único, conforme decisão do
  produto; se todo o espaço de códigos estiver ocupado, não haverá código livre
- A origem do `shortUrl` depende da configuração de hosts permitidos e, quando
  aplicável, de cabeçalhos encaminhados por proxies confiáveis
- Crescimento da tabela de links sem política de limpeza, fora de escopo por ora
