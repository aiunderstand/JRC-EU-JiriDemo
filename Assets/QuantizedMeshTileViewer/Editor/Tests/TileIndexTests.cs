using BruTile;
using NUnit.Framework;
using Terrain.ExtensionMethods;

namespace Terrain.Tests
{
    public class TileIndexTests
    {

        [Test]
        public void TileIndexToStringWorks()
        {
            // arrange
            var t = new TileIndex(100,200,"0");

            // act
            var index = t.ToIndexString();

            // assert
            Assert.IsTrue(index == "0/100/200");
        }

        [Test]
        public void ConvertRDtoWGS84()
        {
            var result = CoordinateHelper.RDtoWGS84(122202, 487250);
            Assert.IsTrue(result[0] == 4.90559760435224);
            Assert.IsTrue(result[1] == 52.372143838117);
        }

        [Test]
        public void ConvertWGS84toRD()
        {
            var result = CoordinateHelper.WGS84toRD(4.90559760435224, 52.372143838117);
            Assert.IsTrue(result[0] == 122202);
            Assert.IsTrue(result[1] == 487250);
        }

    }
    
}

