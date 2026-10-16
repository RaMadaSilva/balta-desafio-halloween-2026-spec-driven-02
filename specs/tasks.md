# Tarefas

Estado: T-01 a T-17 concluídas e testadas.

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| [x] T-01 | Criar projeto e estrutura VSA | Constituição | — | Projeto compila com .NET 10 e xUnit; existem fatias `Features/Links/Create`, `Features/Links/Redirect` e `Features/Links/Stats`, além de `Domain/Links` e `Infrastructure/Persistence` |
| [x] T-02 | Modelar `Link` e configurar persistência | RF-08, RN-01, D-01, D-04 | T-01 | `Domain/Links` define o agregado; `Infrastructure/Persistence` contém `DbContext`, repositório e migração; o banco impõe código único com comparação sensível a maiúsculas/minúsculas |
| [x] T-03 | Gerar código automático na fatia `Create` | RF-02, RN-05, D-03 | T-01 | Testes unitários verificam tamanho e alfabeto `A-Z`, `a-z`, `1-9` |
| [x] T-04 | Validar URL na fatia `Create` | RN-02, caso de borda URL | T-01 | Testes unitários aceitam URL absoluta http/https até 2048 caracteres e rejeitam URL inválida, outro esquema e comprimento superior |
| [x] T-05 | Implementar a fatia `Create` para códigos automáticos | RF-01, RF-02, RF-08, RN-01, RN-06, D-04, D-05 | T-02, T-03, T-04 | Endpoint delega no handler de aplicação; testes unitários confirmam nova geração até obter código livre; integração confirma `201`, persistência e `shortUrl` derivado do esquema/host do pedido, e `400 ProblemDetails` com mensagem em português para URL inválida |
| [x] T-06 | Completar a fatia `Create` com código personalizado | RF-03, RN-01, RN-05, D-04, D-07 | T-05 | Testes unitários cobrem tamanho, alfabeto e unicidade, incluindo distinção entre maiúsculas/minúsculas; integração confirma código válido em `201` e erros em `400 ProblemDetails` com mensagens em português |
| [x] T-07 | Completar a fatia `Create` com expiração | RF-06, RN-07 | T-05 | Testes unitários com instante controlado cobrem valor futuro, atual e passado; integração confirma que valor atual ou passado responde `400 ProblemDetails` com mensagem em português |
| [x] T-08 | Implementar a fatia `Redirect` | RF-04, RF-06, RN-03 | T-05, T-07 | Endpoint delega no handler de aplicação; testes de integração cobrem `302` válido, `404` inexistente e `410` expirado e verificam o limite exacto de expiração em UTC |
| [x] T-09 | Contar acessos na fatia `Redirect` | RF-05, RN-03, RN-04, D-02 | T-08 | Handler delega a actualização atómica no repositório; testes unitários verificam que o contador só aumenta; teste concorrente confirma que cada acesso válido, repetido ou automatizado, é contado sem perdas e que acessos expirados não contam |
| [x] T-10 | Implementar a fatia pública `Stats` | RF-07, D-06 | T-09 | Endpoint delega no handler de aplicação; sem chave ou autenticação, teste de integração confirma `200` com `accessCount` correcto para link válido e expirado, e `404` para código inexistente |
| [x] T-11 | Verificar persistência entre reinícios | RF-08, D-01 | T-02, T-05 | Teste de integração cria um link, reinicia a aplicação e reabre o banco SQLite, confirmando que o link e os seus dados continuam disponíveis |

## Refactor da arquitectura

T-01 a T-11 registam a implementação inicial já concluída. As tarefas seguintes descrevem a refatoração pendente para cumprir D-08; os requisitos RF/RN e os contratos HTTP mantêm-se.

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| [x] T-12 | Isolar o modelo e o contrato de persistência | D-08, RF-08, RN-01, D-01, D-02 | — | LinkRecord, DbContext, migrações e contrato/implementação do repositório ficam em Infrastructure/Persistence; Domain/Links deixa de existir como modelo partilhado; esquema, dados, unicidade e actualização atómica permanecem iguais |
| [x] T-13 | Tornar Create autocontido | D-08, RF-01, RF-02, RF-03, RF-06, RN-02, RN-05, RN-06, RN-07 | T-12 | Endpoint, serviço de aplicação, request/response, geração, validadores e regras de criação ficam em Features/Links/Create; o slice só depende da persistência partilhada e mantém os mesmos resultados HTTP |
| [x] T-14 | Tornar Redirect autocontido | D-08, RF-04, RF-05, RF-06, RN-03, RN-04, D-02 | T-12 | Endpoint, serviço de aplicação, contratos e decisão de validade ficam em Features/Links/Redirect; a persistência executa o incremento condicional atómico; testes mantêm 302, 404, 410 e ausência de contagem após expiração |
| [x] T-15 | Tornar Stats autocontido | D-08, RF-07, D-06 | T-12 | Endpoint, serviço de aplicação, contratos e regra de acesso público ficam em Features/Links/Stats; o slice consulta a persistência sem consumir tipos ou serviços de outros slices; respostas 200 e 404 mantêm-se |
| [x] T-16 | Uniformizar tratamento de erros e observabilidade | D-08, constituição | T-13, T-14, T-15 | Infrastructure/Errors produz 400 ProblemDetails em português para erros de validação; logging permanece configurado uma única vez no host por WebApplication.CreateBuilder e appsettings*; regras e erros específicos continuam nos respectivos slices; não são adicionadas dependências sem justificação |
| [x] T-17 | Verificar fronteiras e regressão funcional | D-08, RF-01 a RF-08, RN-01 a RN-07 | T-12, T-13, T-14, T-15, T-16 | Verificação confirma ausência de referências entre slices no código de produção, testes unitários por regra nos slices ou persistência, testes de integração por endpoint, persistência e concorrência; dotnet test src/Shortener.sln passa e os contratos HTTP permanecem iguais |
