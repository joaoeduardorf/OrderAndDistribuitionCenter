# Pedidos - Docker Compose

Este repositório contém a configuração do Docker Compose para o sistema Distribuition Center. Ele configura e gerencia diversos serviços essenciais para o funcionamento do sistema, incluindo APIs, banco de dados, mensageria e rastreamento distribuído.

## Serviços

Este ambiente Docker Compose inclui os seguintes serviços:

- **distribuitioncenterapi**: API principal do centro de distribuição. Acessível na porta `5000` (mapeada para `8080` internamente). Conectada ao Jaeger para rastreamento distribuído.

- **orderapi**: API de pedidos. Acessível na porta `5002` (mapeada para `8080` internamente). Conectada ao PostgreSQL, Kafka e Jaeger.

- **orderconsumer**: Serviço para consumir mensagens de pedidos do Kafka, conectado ao PostgreSQL e Jaeger para rastreamento. **Este serviço consome mensagens publicadas no Kafka e depende das APIs e de Kafka.**

- **zookeeper**: Serviço Zookeeper, necessário para o Kafka. Porta `2182` (interna `2181`) configurada para permitir o gerenciamento de brokers do Kafka.

- **kafka**: Serviço de mensageria Kafka. Configurado com as portas `29092` (externa) e `9092` (interna, usada por outros contêineres), conectado ao Zookeeper.

- **postgres**: Banco de dados PostgreSQL para armazenamento de dados das APIs. Acessível na porta `5432`.

- **pgadmin**: Interface gráfica para gerenciar o PostgreSQL. Disponível em `http://localhost:5050` com o usuário `admin@admin.com` e senha `admin`.

- **jaeger**: Ferramenta de rastreamento distribuído, com várias portas para gRPC, HTTP e compatibilidade com Zipkin. Interface web acessível em `http://localhost:16686`.

## Requisitos

- **Docker** e **Docker Compose** devem estar instalados no sistema.

## Configuração das Variáveis de Ambiente

As variáveis de ambiente principais estão configuradas diretamente no `docker-compose.yml` e incluem:

- **ASPNETCORE_ENVIRONMENT**: Define o ambiente para as APIs como `Production`.
- **ConnectionStrings__PostgresConnection**: String de conexão para o banco de dados PostgreSQL (`Host=postgres;Database=OrderDB;Username=root;Password=password`).
- **Jaeger__AgentHost** e **Jaeger__AgentPort**: Configurações de conexão com o agente Jaeger (`Host=jaeger`, `Port=6831`).
- **Kafka__BootstrapServers**: Servidor bootstrap do Kafka (`kafka:9092`).

## Uso

Para iniciar o ambiente Docker Compose, execute o seguinte comando na raiz do projeto:

```bash
docker-compose up --build
```
Para parar e remover os contêineres e as redes associadas, execute:
```bash
docker-compose down
```
## Acessos Rápidos

- **Distribuition Center API**: [http://localhost:5000](http://localhost:5000)
- **Order API**: [http://localhost:5002](http://localhost:5002)
- **PgAdmin**: [http://localhost:5050](http://localhost:5050) (usuário: `admin@admin.com`, senha: `admin`)
- **Jaeger UI**: [http://localhost:16686](http://localhost:16686)

## Observações

- **API do Order Consumer**: O serviço `orderconsumer` consome mensagens de pedidos publicadas no Kafka. Ele está configurado para se conectar ao PostgreSQL e Jaeger, e depende da disponibilidade do Kafka para processar mensagens.

- **SSL/HTTPS**: Por padrão, as APIs estão configuradas apenas para HTTP. Caso precise habilitar HTTPS, descomente as portas e variáveis de ambiente HTTPS correspondentes no `docker-compose.yml`.

- **Configuração do PgAdmin**: Para gerenciar o banco de dados PostgreSQL via PgAdmin, crie uma nova conexão com as seguintes credenciais:
  - **Host**: `postgres`
  - **Porta**: `5432`
  - **Usuário**: `root`
  - **Senha**: `password`

## Rede

Todos os serviços estão conectados por uma rede Docker bridge chamada `my_network`, facilitando a comunicação entre os contêineres.

## Notas Adicionais

Para configurar tópicos no Kafka, ajuste as variáveis de ambiente no `orderapi` e `orderconsumer` para definir o tópico desejado e verifique o Kafka para garantir que ele está em operação antes de iniciar o processamento de pedidos.

