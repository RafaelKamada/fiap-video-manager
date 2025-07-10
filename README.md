# 📹: Video Manager
![VideoManagerCompactor](video.png?raw=true "VideoManagerCompactor")

## :pencil: Descrição do Projeto
<p align="left">Este projeto tem como objetivo concluir as  entregas do Tech Challenge do curso de Software Architecture da Pós Graduação da FIAP 2024/2025.
Este repositório constrói um serviço para gerenciamento de vídeos.</p>

## 📊 Code Coverage
[![Coverage Status](https://coveralls.io/repos/github/RafaelKamada/fiap-video-manager/badge.svg?branch=main)](https://coveralls.io/github/RafaelKamada/fiap-video-manager?branch=main)

 
## 🏗️ Arquitetura de Microsserviços
![Arquitetura](arquitetura.png?raw=true&cachebuster=v2 "Arquitetura")

### :computer: Tecnologias Utilizadas
- Linguagem escolhida: .NET
- Banco de Dados: PostgreSQL

### :hammer: Detalhes desse serviço
Serviço responsável pelo gerenciamento de vídeos da FIAP X, desenvolvido em .NET e PostgreSQL.

### :hammer_and_wrench: Execução do projeto
Para rodar o serviço localmente, você precisa ter Docker e .NET 8 instalados.

Para construir e rodar o serviço, utilize o comando:

```bash
docker-compose up --build -d
```

Esse comando irá:

* Criar a rede Docker para comunicação entre os serviços.
* Subir o banco de dados PostgreSQL.
* Iniciar o serviço `videomanager.api`.

Para parar e remover os containers, use:

```bash
docker-compose down
```

### Endpoints Disponíveis

| Método | Endpoint                                | Descrição                                             |
| ------ | --------------------------------------- | ----------------------------------------------------- |
| POST   | /api/Videos/upload                      | Realiza upload do video para fila de processamento.   |
| PUT    | /api/Videos/status/{id}                 | Atualiza o status do processamento de video.          |  
| GET    | /api/Videos/{id}                        | Realiza download de um zip de video pelo seu ID.      | 
| GET    | /api/Videos/status/{usuario}            | Consulta listagem de status dos vídeos de um usuário. |
| POST   | /api/Auth/login                         | Realiza autenticação.                                 |

### 🔑 Autenticação
A aplicação utiliza autenticação baseada em JWT (JSON Web Token).

Quando um usuário faz login, a API gera um token JWT contendo informações como email e roles (caso implementado).

Esse token é assinado com uma chave secreta definida nas configurações e possui um tempo de expiração.

O token gerado deve ser enviado no header Authorization das requisições protegidas, no formato:

Authorization: Bearer {seu_token_aqui}
A API valida esse token em cada requisição para garantir que:

Ele está assinado com a chave correta.

Não está expirado.

Possui o issuer e audience esperados.

Essa autenticação garante segurança e controle de acesso nos endpoints protegidos da aplicação.

```
{
    "user": "VideoCompactor",
    "password": "$Glsop34254@"
}
```

### 🗄️ Outros repos do microserviço dessa arquitetura
- [Video Compactor](https://github.com/diegogl12/video-compactor) 

### :page_with_curl: Documentações
- [Miro (todo planejamento do projeto)](https://miro.com/app/board/uXjVKhyEAME=/)


### :busts_in_silhouette: Autores
| [<img loading="lazy" src="https://avatars.githubusercontent.com/u/96452759?v=4" width=115><br><sub>Robson Vilaça - RM358345</sub>](https://github.com/vilacalima) |  [<img loading="lazy" src="https://avatars.githubusercontent.com/u/16946021?v=4" width=115><br><sub>Diego Gomes - RM358549</sub>](https://github.com/diegogl12) |  [<img loading="lazy" src="https://avatars.githubusercontent.com/u/8690168?v=4" width=115><br><sub>Nathalia Freire - RM359533</sub>](https://github.com/nathaliaifurita) |  [<img loading="lazy" src="https://avatars.githubusercontent.com/u/43392619?v=4" width=115><br><sub>Rafael Kamada - RM359345</sub>](https://github.com/RafaelKamada) |
| :---: | :---: | :---: | :---: |
