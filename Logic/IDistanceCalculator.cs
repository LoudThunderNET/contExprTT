using ContinetExpress.TT.Logic.Models;

namespace ContinetExpress.TT.Logic
{
    /// <summary>
    /// Калькулятор расстояния по географическим координатам.
    /// </summary>
    public interface IDistanceCalculator
    {
        /// <summary>
        /// Вычисляет примерное расстояние между двумя точка заданными географическими координатами
        /// </summary>
        /// <param name="a">Координаты первой точки</param>
        /// <param name="b">Координаты второй точки</param>
        /// <returns>Расстояние в метрах</returns>
        double Calculate(Location a, Location b);
    }

    /// <inheritdoc cref="IDistanceCalculator"/>
    public class DistanceCalculator : IDistanceCalculator
    {
        /// <summary>
        /// Средний радиус земли
        /// </summary>
        private const double AvrEarthRadius = 6371;

        public double Calculate(Location a, Location b)
        {
            double f1 = GradToRad(a.Lat);
            double f2 = GradToRad(b.Lat);
            double d = Math.Sin(f1) * Math.Sin(f2) + Math.Cos(f1) * Math.Cos(f2) * Math.Cos(GradToRad(a.Lon) - GradToRad(b.Lon));

            return Math.Acos(d) * AvrEarthRadius;

            double GradToRad(double grad) => grad * Math.PI / 180;
        }
    }
}