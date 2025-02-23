# Spy Game - MAUI .NET

Este é um projeto de estudo desenvolvido com MAUI .NET, implementando um jogo de espião que utiliza sockets para comunicação em rede local. O jogo inclui tanto o cliente quanto o servidor, usando `TcpClient` e `TcpListener`.

## Objetivo
O projeto foi criado com o intuito de explorar a tecnologia MAUI .NET, focando em conceitos de desenvolvimento de aplicativos multiplataforma e comunicação de rede via sockets.

## Funcionalidades
- **Comunicação via Socket:** Implementação de cliente e servidor usando `TcpClient` e `TcpListener`.
- **Apenas Rede Local:** O jogo foi projetado para funcionar exclusivamente em redes locais.
- **Restrições de Conexão:** Não permite conexões de clientes com o mesmo IP.
- **Foreground Service (Android):** No Android, o jogo utiliza um serviço em foreground, permitindo que a tela do dispositivo seja bloqueada enquanto o jogo continua ativo.
- **Mecanismo de Reespera:** Implementado um mecanismo de espera para reconexão, evitando falhas de comunicação via socket.
- **Alertas de Conexão:** Notificações para:
  - Falta de conexão em rede local.
  - Problemas de conexão com o servidor.

## Requisitos
- .NET 8 ou superior.
- MAUI .NET Workload instalado.
- Rede local configurada corretamente para testes.

## Como Executar
1. Clone o repositório.
2. Abra o projeto no Visual Studio com suporte a MAUI .NET.
3. Compile e execute tanto o cliente quanto o servidor.
4. Certifique-se de que os dispositivos estão na mesma rede local.

## Contribuições
Este projeto foi desenvolvido para fins de estudo, mas contribuições são bem-vindas!
