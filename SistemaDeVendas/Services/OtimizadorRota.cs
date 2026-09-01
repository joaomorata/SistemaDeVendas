namespace SistemaDeVendas.Services
{
    public class OtimizadorRota
    {
        public class Ponto
        {
            public int ClienteId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public (List<Ponto> Ordem, double DistanciaKm) Resolver(double latPartida, double lonPartida, List<Ponto> pontos)
        {
            var ordem = VizinhoMaisProximo(latPartida, lonPartida, pontos);
            ordem = DoisOpt(latPartida, lonPartida, ordem);
            return (ordem, DistanciaTotal(latPartida, lonPartida, ordem));
        }

        private List<Ponto> VizinhoMaisProximo(double latPartida, double lonPartida, List<Ponto> pontos)
        {
            var restantes = new List<Ponto>(pontos);
            var ordem = new List<Ponto>();

            var latAtual = latPartida;
            var lonAtual = lonPartida;

            while (restantes.Count > 0)
            {
                var proximo = restantes[0];
                var menor = Distancia(latAtual, lonAtual, proximo.Latitude, proximo.Longitude);

                foreach (var ponto in restantes)
                {
                    var d = Distancia(latAtual, lonAtual, ponto.Latitude, ponto.Longitude);
                    if (d < menor)
                    {
                        menor = d;
                        proximo = ponto;
                    }
                }

                ordem.Add(proximo);
                restantes.Remove(proximo);
                latAtual = proximo.Latitude;
                lonAtual = proximo.Longitude;
            }

            return ordem;
        }

        private List<Ponto> DoisOpt(double latPartida, double lonPartida, List<Ponto> rota)
        {
            var melhor = new List<Ponto>(rota);
            var melhorou = true;

            while (melhorou)
            {
                melhorou = false;

                for (var i = 0; i < melhor.Count - 1; i++)
                {
                    for (var j = i + 1; j < melhor.Count; j++)
                    {
                        var nova = new List<Ponto>(melhor);
                        nova.Reverse(i, j - i + 1);

                        if (DistanciaTotal(latPartida, lonPartida, nova) < DistanciaTotal(latPartida, lonPartida, melhor))
                        {
                            melhor = nova;
                            melhorou = true;
                        }
                    }
                }
            }

            return melhor;
        }

        private double DistanciaTotal(double latPartida, double lonPartida, List<Ponto> rota)
        {
            if (rota.Count == 0)
                return 0;

            var total = Distancia(latPartida, lonPartida, rota[0].Latitude, rota[0].Longitude);

            for (var i = 0; i < rota.Count - 1; i++)
                total += Distancia(rota[i].Latitude, rota[i].Longitude, rota[i + 1].Latitude, rota[i + 1].Longitude);

            return total;
        }

        private double Distancia(double lat1, double lon1, double lat2, double lon2)
        {
            const double raioTerra = 6371;

            var dLat = EmRadianos(lat2 - lat1);
            var dLon = EmRadianos(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(EmRadianos(lat1)) * Math.Cos(EmRadianos(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return raioTerra * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private double EmRadianos(double graus) => graus * Math.PI / 180;
    }
}
