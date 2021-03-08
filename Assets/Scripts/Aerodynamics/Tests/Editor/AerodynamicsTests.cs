using NUnit.Framework;

namespace Perrinn424.AerodynamicsSystem.Editor.Tests
{
    public class AerodynamicsTests
    {
        // Verified by https://www.translatorscafe.com/unit-converter/en-US/calculator/altitude/
        // http://nurburgring.org.uk/altitude-profile.php
        [TestCase(0f, 1.225f)] //level sea
        [TestCase(667f, 1.1484f)] //Madrid
        [TestCase(620f, 1.153735f)] //Nürburgring most altitude
        [TestCase(320f, 1.187809f)] //Nürburgring least altitude
        public void AtmosphereTest(float altitude, float expectedDensity)
        {
            Atmosphere atmosphere = new Atmosphere();
            float density = (float)atmosphere.UpdateAtmosphere(altitude, 0f);
            Assert.That(expectedDensity, Is.EqualTo(density).Within(10e-2));
        }

        [Test]
        public void AltitudeConverterTest()
        {
            var altitudeConverter = new AltitudeConverter(620f,80.32f);
            Assert.That(620f, Is.EqualTo(altitudeConverter.ToAltitude(80.32f)));
            Assert.That(80.32f, Is.EqualTo(altitudeConverter.ToUnity(620f)));
        }
    }
}
