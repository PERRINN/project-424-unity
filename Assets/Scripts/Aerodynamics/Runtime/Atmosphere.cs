using System;

namespace Perrinn424.AerodynamicsSystem
{
    public class Atmosphere
    {
        // Constants for the atmosphere model
        private const double tHeight = 11000;            // Tropopause Height [m]
        private const double e = 4.25587981;       // Gas Constant Ratio
        private const double rho0 = 1.225;            // Sea Level air density at ISA [m/s]
        private const double vs0 = 340.294005808213; // Sea Level speed of sound [m/s]
        private const double mu0 = 1.7894E-005;      // Sea Level air viscosity

        private double T_std = 0;
        private double Rho_Rho0 = 0;
        private double Nhp = 0;
        private double Tisa = 0;
        private double T_kelvin = 0;
        private double H = 0;
        private double Rho_Std = 0;
        private double P_P0 = 0;
        private double T_T0 = 0;
        private double DhpDh = 0;
        private double Vsound = 0;
        private double Rho = 0;
        private double Sqrt_Sigma = 0;
        private double mu = 0;

        /// <summary>
        /// Density, in kg/m³
        /// </summary>
        public double Density { get; private set; }


        /// <summary>
        /// Calculates the air density at a specific geometric altitude and delta ISA temperature
        /// </summary>
        /// <param name="alt">Altitude, in meters</param>
        /// <param name="deltaISA">delta temperature, in degC</param>
        /// <returns>Density, in kg/m³</returns>
        public double UpdateAtmosphere(float alt, float deltaISA)
        {
            Tisa = deltaISA;
            H = alt / tHeight;

            if (H < 1)
            {
                T_std = 288.15 - 71.5 * H;
                P_P0 = Math.Pow(1.0 - 0.248134652090925 * H, e + 1);
                Rho_Rho0 = Math.Pow(1.0 - 0.248134652090925 * H, e);
            }
            else  // I'm assuming we'll never go this route ;)
            {
                T_std = 216.65;
                Nhp = Math.Exp(-1.73457 * H);
                P_P0 = Nhp * 1.26567;
                Rho_Rho0 = Nhp * 1.68338;
            }
            T_kelvin = Tisa + T_std;
            T_T0 = T_kelvin / 288.15;
            DhpDh = T_std / (T_std + Tisa);
            Vsound = Math.Sqrt(T_T0) * vs0;
            Rho_Std = Rho_Rho0 * rho0;
            Rho = Rho_Std / (1 + Tisa / T_std);
            Sqrt_Sigma = Math.Sqrt(Rho / rho0);
            mu = mu0 * (Math.Pow(T_kelvin, 1.5) / (T_kelvin + 120) / 11.984);

            Density = Rho;
            return Density;
        }
    }
}

