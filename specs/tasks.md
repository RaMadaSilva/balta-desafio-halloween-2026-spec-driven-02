# Tarefas

Estado: todas as tarefas T-01 a T-11 foram concluídas e testadas; cada tarefa tem um commit próprio no histórico Git.

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
