# Plano técnico

## Contexto e âmbito
Este plano implementa a especificação funcional do encurtador e respeita a constituição, que prevalece em qualquer conflito.

A alteração actual redefine a organização interna para uma Vertical Slice Architecture estrita. Não altera os contratos HTTP, as regras RF/RN, os dados persistidos nem os resultados esperados pelos utilizadores.

## Arquitectura alvo
- Estilo: Vertical Slice Architecture. Cada slice representa um caso de uso autocontido e contém o endpoint, serviço de aplicação, contratos request/response e regras de domínio próprias. Os testes correspondentes ficam na pasta espelhada Shortener.Tests/Features/Links/<Slice>.
- Os slices Create, Redirect e Stats não referenciam namespaces, tipos, serviços, contratos ou regras uns dos outros. O único ponto que pode conhecer e compor todos os slices é o composition root em Program.cs.
- A lógica e os tipos específicos de um caso de uso ficam na pasta Features/Links/<Slice>. A estrutura de produção contém também Infrastructure/Persistence e Infrastructure/Errors. O logging usa os mecanismos integrados do host ASP.NET Core e não requer um módulo Infrastructure/Observability próprio enquanto não houver instrumentação adicional. Não há Domain/Links partilhado com entidades ou regras de negócio comuns aos slices.
- Só se partilha o que é transversal: persistência, tratamento comum de erros e observabilidade. Não se criam serviços de aplicação, validadores, modelos de domínio, contratos HTTP ou helpers genéricos partilhados.
- Infrastructure/Persistence contém o modelo de persistência sem comportamento de domínio, DbContext, migrações e contrato/implementação do repositório. Esse limite pode ser usado pelos três slices e não expõe serviços de aplicação de outros slices.
- Cada slice traduz os resultados de persistência nos seus próprios resultados e contratos. O modelo de persistência não é a resposta HTTP nem define regras de negócio.
- Os endpoints fazem binding HTTP e delegam no serviço de aplicação da sua própria fatia; não acedem directamente ao DbContext e não implementam regras de negócio.
- O tratamento comum de erros fica num ponto partilhado da fronteira HTTP. Os erros específicos continuam a ser produzidos pelo slice correspondente e são traduzidos para o formato ProblemDetails exigido pela constituição.
- O logging comum fica configurado uma única vez no host, através de WebApplication.CreateBuilder e das definições Logging em appsettings*. Os slices não configuram logging individualmente. Tracing, métricas ou instrumentação adicional só serão introduzidos quando houver requisito e justificação no plano.
- Program.cs compõe endpoints, serviços dos slices e infraestrutura partilhada. Essa referência de composição não constitui uma dependência entre slices.
- TimeProvider é uma abstracção da plataforma .NET injectada nos serviços que precisam de um instante controlado; não é um serviço de aplicação partilhado.
- Não introduzir Mediator, dispatcher genérico, bus de mensagens ou dependências novas para implementar VSA.

## Lacuna arquitectural a corrigir
A implementação actual coloca Link e ILinkRepository em Domain/Links, e os três slices dependem desses tipos. Link também contém expressões de actividade e incremento do contador. A refatoração deve mover o modelo e o contrato de persistência para Infrastructure/Persistence, retirar desse modelo comportamento de domínio e deixar cada slice possuir as suas regras e contratos. A condição SQL atómica necessária para não contar acessos expirados continua na persistência como garantia de consistência dos dados.

A constituição não precisa de alteração: já exige que endpoints não acedam à base de dados directamente e que regras de negócio não vivam nos endpoints nem nos handlers HTTP. A especificação funcional também não muda, porque os requisitos e contratos observáveis permanecem iguais.

## Dependências e justificações
- Microsoft.EntityFrameworkCore.Sqlite: persistência SQLite exigida por RF-08 e actualização atómica do contador exigida por RF-05.
- Microsoft.AspNetCore.Mvc.Testing: testes HTTP de integração dos endpoints Minimal API.
- Microsoft.NET.Test.Sdk, xunit e xunit.runner.visualstudio: execução dos testes xUnit exigidos pela constituição.
- Não se adiciona EF Design nem dotnet-ef: as migrações continuam versionadas no código e aplicadas por Database.Migrate().
- TimeProvider pertence ao .NET 10; não requer pacote adicional.
- Não se introduzem Mediator, dispatcher, bibliotecas de validação ou telemetria externa.

## Decisões
- D-01: SQLite como armazenamento, porque RF-08 exige persistência entre reinícios.
- D-02: cada acesso válido incrementa AccessCount com um UPDATE atómico na persistência. A condição de expiração usa o mesmo instante UTC do slice Redirect para que acessos expirados não sejam contados, conforme RF-05, RN-03 e RN-04.
- D-03: os códigos automáticos têm 7 caracteres sorteados de A-Z, a-z e 1-9, conforme RF-02.
- D-04: um índice único com comparação sensível a maiúsculas e minúsculas garante RN-01. O slice Create traduz colisão personalizada para 400 ProblemDetails e repete a geração automática até inserir um código livre.
- D-05: shortUrl é construído com o esquema e o host externos do pedido de criação, seguidos de /{code}, conforme RF-01. O middleware processa X-Forwarded-Proto e X-Forwarded-Host apenas de IPs configurados em KnownProxies, enquanto AllowedHosts restringe os hosts aceites.
- D-06: as estatísticas são públicas para qualquer pessoa que conheça o código, conforme RF-07; não existe chave de gestão nem autenticação.
- D-07: todos os erros de validação, incluindo código personalizado duplicado, URL inválida e expiração passada, respondem 400 ProblemDetails em português, conforme a constituição.
- D-08: cada slice possui endpoint, serviço de aplicação, request/response e regras de domínio próprios. Só persistência, tratamento comum de erros e observabilidade são partilhados; nenhum slice conhece outro.

## Modelo de persistência
O modelo LinkRecord representa apenas os dados persistidos: Id, Code, TargetUrl, CreatedAtUtc, ExpiresAtUtc e AccessCount. Fica em Infrastructure/Persistence, sem regras de domínio ou métodos de caso de uso. O esquema SQLite, a unicidade sensível a maiúsculas/minúsculas e as migrações existentes são preservados.

## Contratos HTTP
- POST /api/links
  - Entrada: targetUrl, code opcional e expiresAtUtc opcional em ISO 8601 UTC terminado em Z.
  - Sucesso: 201 com { shortUrl, code }; shortUrl usa o esquema e o host externos do pedido.
  - Erro de validação: 400 ProblemDetails.
- GET /{code}
  - Link válido: incrementa o contador uma vez e responde 302 para TargetUrl.
  - Código inexistente: 404; link expirado: 410, sem incrementar o contador.
- GET /api/links/{code}/stats
  - Sem autenticação ou header de chave.
  - Código existente, mesmo expirado: 200 com { accessCount }.
  - Código inexistente: 404.

## Cobertura de qualidade
- Cada regra de negócio RN-01 a RN-07 tem teste unitário no slice responsável ou teste unitário da configuração de persistência que a impõe.
- Cada endpoint tem um teste de integração de caminho feliz: criação 201, redireccionamento 302 e estatísticas 200.
- Testes de integração cobrem erros de validação em 400 ProblemDetails, redireccionamento expirado em 410, código inexistente em 404, contagem concorrente e persistência após reinício.
- Uma verificação arquitectural confirma que nenhum código de produção num slice referencia namespaces de outro slice; apenas Program.cs pode compor todos os slices. Testes de integração podem chamar os contratos HTTP de vários slices sem referenciar os seus tipos internos.
- A suite confirma que a refatoração não altera os contratos HTTP, dados existentes, migrações ou comportamento RF/RN.

## Riscos
- A geração automática repete até inserir um código único; se todo o espaço estiver ocupado, não haverá código livre.
- A origem de shortUrl depende da configuração de hosts permitidos e, quando aplicável, de cabeçalhos encaminhados por proxies confiáveis.
- Crescimento da tabela de links sem política de limpeza continua fora de âmbito.
