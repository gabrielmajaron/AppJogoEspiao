﻿
namespace AppEspiaoJogo.Game
{
    public static class Locations
    {
        private static readonly List<string> _locations = new List<string>
        {
            "Cidade",
            "Castelo",
            "Floresta",
            "Montanha",
            "Praia",
            "Deserto",
            "Vulcão",
            "Caverna",
            "Ilha",
            "Oceano",
            "Ponte",
            "Parque",
            "Campo",
            "Mar",
            "Praça",
            "Farol",
            "Fábrica",
            "Polo Norte",
            "Estádio de Futebol",
            "Biblioteca",
            "Mina de Ouro",
            "Zoológico",
            "Aeroporto",
            "Cruzeiro Marítmo",
            "Fazenda",
            "Base Militar",
            "Laboratório",
            "Arena de Luta",
            "Parque de Diversões",
            "Prisão",
            "Hospital",
            "Olimpíadas",
            "Salão de Beleza",
            "Escritório",
            "Laboratório",
            "Sala de Aula",
            "Cozinha",
            "Estacionamento",
            "Mansão",
            "Praça de Alimentação",
            "Pet Shop",
            "Igreja",
            "Avião",
            "Cassino",
            "Delegacia de Polícia",
            "Teatro",
            "Navio Pirata",
            "Restaurante",
            "Aquário Turístico",
            "Cemitério",
            "Supermercado",
            "Submarino",
            "Nave Espacial",
            "Labirinto",
            "Ilha Deserta"
        };

        public static List<string> GetAll()
        {
            return new List<string>(_locations);
        }

        public static string GetRandomLocation()
        {
            Random random = new Random();
            int index = random.Next(_locations.Count);
            return _locations[index];
        }
    }

}
