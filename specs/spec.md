# Encurtador de links

## Problema
Links longos quebram em mensagens, e-mails e materiais impressos, e não dá para
medir quantas pessoas clicaram neles.

## Objetivo
Permitir que uma pessoa transforme uma URL longa num link curto, partilhe esse
link e acompanhe quantos acessos recebeu.

## Usuários
- Criador do link: gera links curtos e acompanha os acessos
- Visitante: clica no link curto e chega ao destino

## Histórias
- Como criador, quero encurtar uma URL para partilhar um link menor
- Como criador, quero escolher o código do link para usar um nome legível
- Como criador, quero ver quantos acessos o meu link recebeu
- Como visitante, quero ser redirecionado ao destino sem passar por uma tela intermédia

## Requisitos funcionais
- RF-01: o sistema recebe uma URL de destino e devolve o código e o link curto
  completo, usando o esquema e o host do pedido de criação
- RF-02: o código automático tem 7 caracteres gerados aleatoriamente, usando
  apenas letras ASCII de A a Z, letras ASCII de a a z e números de 1 a 9
- RF-03: o criador pode informar um código próprio de 4 a 20 caracteres, usando
  apenas letras ASCII de A a Z, letras ASCII de a a z e números de 1 a 9
- RF-04: aceder a um link curto existente redireciona para a URL de destino
- RF-05: cada acesso a um link válido incrementa o contador, incluindo acessos
  repetidos e automatizados
- RF-06: o criador pode definir um instante de expiração em UTC; o link é válido
  apenas enquanto o instante UTC atual for anterior ao instante de expiração
- RF-07: qualquer pessoa que conheça o código pode consultar o total de acessos
- RF-08: os links e os seus dados são persistidos para sobreviverem ao reinício
  da aplicação

## Regras de negócio
- RN-01: o código do link é único em todo o sistema, com comparação sensível a
  maiúsculas e minúsculas (`ABC` e `abc` são códigos diferentes)
- RN-02: a URL de destino tem de ser absoluta e sintaticamente válida, e usar
  apenas o esquema http ou https; o sistema não verifica se o destino é acessível
  pela rede
- RN-03: um link está expirado quando o instante UTC atual é igual ou posterior
  ao seu instante de expiração; um link expirado não redireciona nem conta acesso
- RN-04: o contador de acessos nunca diminui
- RN-05: códigos automáticos e personalizados aceitam somente letras ASCII
  A-Z, letras ASCII a-z e dígitos de 1 a 9; o dígito 0, espaços e caracteres
  especiais não são permitidos
- RN-06: se um código automático gerado já estiver em uso, o sistema gera outro
  e repete a verificação até obter um código livre
- RN-07: o instante de expiração, quando informado, tem de ser um instante UTC
  no formato ISO 8601 terminado em Z e estritamente posterior ao instante atual

## Casos de borda
- Código personalizado já em uso: recusar a criação com erro de validação,
  mensagem em português e resposta `400 ProblemDetails`, conforme a constituição
- Código automático já em uso: gerar outro código e repetir até obter um livre
- Código inexistente no redireccionamento ou nas estatísticas: responder `404`
- URL inválida, com esquema diferente de http/https ou com mais de 2048
  caracteres: recusar com `400 ProblemDetails`
- Destino privado ou inacessível pela rede: não é verificado; basta validar o
  formato absoluto e o esquema
- Instante de expiração igual ou anterior ao instante atual na criação: recusar
  com `400 ProblemDetails`
- Acesso no instante de expiração ou depois dele: responder `410` e não contar
  acesso
- Link expirado: as estatísticas continuam disponíveis enquanto o código existir

## Fora de escopo
- Cadastro e login de usuários
- Domínio personalizado
- Relatório com origem geográfica do acesso
- Edição do destino de um link já criado
- Verificação de acessibilidade do destino pela rede

## Critérios de aceite
- Criar um link devolve `code` e `shortUrl`; o `shortUrl` usa o esquema e o host
  do pedido de criação
- Códigos automáticos têm 7 caracteres no alfabeto permitido e colisões fazem
  a geração repetir até encontrar um código livre
- Códigos personalizados aceitam apenas o alfabeto permitido e o comprimento
  de 4 a 20; valores inválidos ou duplicados respondem `400 ProblemDetails`
- Dois links não podem ter o mesmo código; códigos que diferem apenas entre
  maiúsculas e minúsculas são distintos
- URL absoluta http/https válida até 2048 caracteres é aceite sem verificação
  de acesso à rede; URL inválida é rejeitada com `400 ProblemDetails`
- Expiração só aceita um instante UTC futuro; o link funciona enquanto
  `nowUtc < expiresAtUtc` e responde `410` a partir de `nowUtc >= expiresAtUtc`
- Um link expirado não incrementa o contador
- Cada acesso válido incrementa o contador, incluindo acessos repetidos e
  automatizados
- Qualquer pessoa que conheça o código pode consultar o total, mesmo após a
  expiração do link
- Após reiniciar a aplicação, os links persistidos continuam a funcionar
